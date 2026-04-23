using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace AutomatedETags
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Microsoft.Net.Http.Headers;

    namespace AutomatedETags
    {
        public static class ETagValidator
        {
            public static bool IsPreconditionValidWithHash(HttpContext context, string currentDataString)
            {
                if (!context.Request.Headers.TryGetValue(HeaderNames.IfMatch, out var ifMatch))
                {
                    return true;
                }
                var options = context.RequestServices.GetRequiredService<IOptions<ETagOptions>>().Value;
                string currentHash = ETagHashHelper.GenerateRawHashFromString(currentDataString, options.Algorithm);
                var formattedDbHash = $"\"{currentHash}\"";

                var clientETag = ifMatch.ToString();
                if (clientETag.StartsWith("W/"))
                {
                    clientETag = clientETag.Substring(2);
                }

                return clientETag == formattedDbHash;
            }
        }
    }
}
