using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PodNoms.Data.Models.Settings;
using PodNoms.Api.Persistence;

namespace PodNoms.Api.Services.Middleware {
    public sealed class CustomDomainRedirectOptions {

    }
    public sealed class CustomDomainRedirectMiddleware {
        private readonly RequestDelegate _next;
        private readonly AppSettings _appSettings;
        private readonly ILogger<CustomDomainRedirectMiddleware> _logger;

        public CustomDomainRedirectMiddleware(RequestDelegate next,
            IOptions<AppSettings> appSettings,
            ILogger<CustomDomainRedirectMiddleware> logger) {
            _next = next;
            _appSettings = appSettings.Value;
            _logger = logger;
        }
        public async Task Invoke(HttpContext context, IPodcastRepository repository) {
            var requestHost = context.Request.Host.Host;
            var siteHost = new UriBuilder(_appSettings.SiteUrl).Host;
            var cleaned = new System.Uri(_appSettings.RssUrl).GetComponents(
                    UriComponents.AbsoluteUri & ~UriComponents.Port & ~UriComponents.Scheme, UriFormat.UriEscaped)
                    .TrimEnd('/');
            if (requestHost.Equals(cleaned)) {
                var redirectUrl = $"{_appSettings.CanonicalRssUrl}{context.Request.Path}";
                context.Response.Redirect(redirectUrl, false);
                return;
            } else if (!requestHost.Equals(siteHost)) {
                //we're on a custom domain, check for matches
                var candidate = await repository.GetAll()
                    .Where(r => r.CustomDomain == requestHost)
                    .Include(r => r.AppUser)
                    .FirstOrDefaultAsync();
                if (candidate != null) {
                    var redirectUrl = $"{_appSettings.RssUrl}{candidate.AppUser.Slug}/{candidate.Slug}";
                    context.Response.Redirect(redirectUrl);
                    return;
                }
            }
            await _next(context);
        }
    }
}
