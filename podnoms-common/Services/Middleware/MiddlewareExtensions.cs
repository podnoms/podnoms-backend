using Microsoft.AspNetCore.Builder;

namespace PodNoms.Common.Services.Middleware {
    public static class MiddlewareExtensions {
        public static IApplicationBuilder UseCustomDomainRedirect(this IApplicationBuilder app) {
            return app
                .UseMiddleware<CustomDomainRedirectMiddleware>()
                .UseMiddleware<SharingLinkRedirectMiddleware>();
        }
    }
}