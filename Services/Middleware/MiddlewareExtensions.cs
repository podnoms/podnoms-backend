using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;

namespace PodNoms.Api.Services.Middleware {
    public static class MiddlewareExtensions {
        public static IApplicationBuilder UseCustomDomainRedirect(this IApplicationBuilder app) {
            return app.UseMiddleware<CustomDomainRedirectMiddleware>();
        }
    }
}