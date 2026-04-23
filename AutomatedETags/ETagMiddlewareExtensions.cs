using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace AutomatedETags
{
    public static class ETagMiddlewareExtensions
    {
        // Allows developers to configure the options (IgnoredRoutes, Mode)
        public static IServiceCollection AddAutomatedETags(this IServiceCollection services, Action<ETagOptions>? configureOptions = null)
        {
            if (configureOptions != null)
            {
                services.Configure(configureOptions);
            }
            return services;
        }

        public static IApplicationBuilder UseAutomatedETags(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ETagMiddleware>();
        }
    }
}
