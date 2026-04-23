using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using System.IO.Hashing;
using System.Security.Cryptography;
using System.Text;

namespace AutomatedETags
{
    public class ETagMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ETagOptions _options;
        private readonly ILogger<ETagMiddleware> _logger;

        public ETagMiddleware(RequestDelegate next, IOptions<ETagOptions> options, ILogger<ETagMiddleware> logger)
        {
            _next = next;
            _options = options.Value;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!ShouldProcessRequest(context, out var etagAttr))
            {
                _logger.LogDebug("Skipping ETag generation for path: {Path}", context.Request.Path);
                await _next(context);
                return;
            }

            Stream originalBodyStream = context.Response.Body;
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            await _next(context);

            if (IsResponseEligibleForETag(context, responseBody))
            {
                var serverETag = await GenerateETagAsync(context, responseBody, etagAttr);

                if (IsClientCacheValid(context, serverETag))
                {
                    _logger.LogInformation(
                        "ETag match found '{ETag}' for {Path}. Returning 304 Not Modified. Saved {Bytes} bytes of bandwidth.",
                        serverETag.ToString(),
                        context.Request.Path,
                        responseBody.Length);

                    context.Response.StatusCode = StatusCodes.Status304NotModified;
                    context.Response.Body = originalBodyStream;
                    return;
                }

                _logger.LogDebug("Generated new ETag '{ETag}' for {Path}", serverETag.ToString(), context.Request.Path);
                context.Response.Headers[HeaderNames.ETag] = serverETag.ToString();
            }

            responseBody.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalBodyStream);
        }

        private bool ShouldProcessRequest(HttpContext context, out EnableETagAttribute etagAttr)
        {
            etagAttr = null;
            if (context.Request.Method != HttpMethods.Get) return false;

            var endpoint = context.GetEndpoint();
            etagAttr = endpoint?.Metadata.GetMetadata<EnableETagAttribute>();
            bool hasSkipTag = endpoint?.Metadata.GetMetadata<SkipETagAttribute>() != null;

            return _options.Mode == ETagMode.OptIn ? etagAttr != null : !hasSkipTag;
        }

        private bool IsResponseEligibleForETag(HttpContext context, MemoryStream responseBody)
        {
            if (context.Response.StatusCode != StatusCodes.Status200OK) return false;

            if (responseBody.Length > _options.MaxBodySize)
            {
                _logger.LogWarning("Response body for {Path} exceeded MaxBodySize ({Size} bytes). ETag bypassed.", context.Request.Path, responseBody.Length);
                return false;
            }

            return true;
        }

        private async Task<EntityTagHeaderValue> GenerateETagAsync(
            HttpContext context,
            MemoryStream responseBody,
            EnableETagAttribute etagAttr)
        {
            string contentToHash;

            if (context.Items.TryGetValue("CustomETagValue", out var manualValue) && manualValue != null)
            {
                contentToHash = manualValue.ToString();
            }
            else
            {
                responseBody.Seek(0, SeekOrigin.Begin);
                using var reader = new StreamReader(responseBody, Encoding.UTF8, leaveOpen: true);
                contentToHash = await reader.ReadToEndAsync();
            }

            string rawHash = ETagHashHelper.GenerateRawHashFromString(contentToHash, _options.Algorithm);
            bool isWeak = etagAttr?.IsWeak ?? _options.UseWeakETags;

            return new EntityTagHeaderValue($"\"{rawHash}\"", isWeak);
        }

        private bool IsClientCacheValid(HttpContext context, EntityTagHeaderValue serverETag)
        {
            var clientETags = context.Request.GetTypedHeaders().IfNoneMatch;
            return clientETags != null && clientETags.Any(c => c.Compare(serverETag, useStrongComparison: false));
        }
    }
}