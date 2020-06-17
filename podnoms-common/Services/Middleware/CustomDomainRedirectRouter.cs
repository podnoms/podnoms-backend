using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PodNoms.Common.Data.Settings;
using PodNoms.Common.Persistence.Repositories;

namespace PodNoms.Common.Services.Middleware {

    public class CustomDomainRouteTransformer : DynamicRouteValueTransformer {
        private readonly IPodcastRepository _podcastRepository;
        private readonly ILogger<CustomDomainRedirectMiddleware> _logger;
        private readonly AppSettings _appSettings;

        public CustomDomainRouteTransformer(IPodcastRepository podcastRepository,
                            ILogger<CustomDomainRedirectMiddleware> logger,
                            IOptions<AppSettings> appSettings) {
            _podcastRepository = podcastRepository;
            _logger = logger;
            _appSettings = appSettings.Value;
        }

        public override async ValueTask<RouteValueDictionary> TransformAsync(
            HttpContext httpContext, RouteValueDictionary values) {
            var requestHost = httpContext.Request.Host.Host;
            var siteHost = new UriBuilder(_appSettings.SiteUrl).Host;
            var cleaned = new Uri(_appSettings.RssUrl).GetComponents(
                    UriComponents.AbsoluteUri & ~UriComponents.Port & ~UriComponents.Scheme, UriFormat.UriEscaped)
                    .TrimEnd('/');
            if (requestHost.Equals(cleaned)) {
                var redirectUrl = Flurl.Url.Combine(
                    _appSettings.CanonicalRssUrl,
                    httpContext.Request.Path);
                httpContext.Response.Redirect(redirectUrl, false);
            } else if (!requestHost.Equals(siteHost)) {
                try {
                    //we're on a custom domain, check for matches
                    var candidate = await _podcastRepository.GetAll()
                        .Where(r => r.CustomDomain == requestHost)
                        .Include(r => r.AppUser)
                        .FirstOrDefaultAsync();
                    if (candidate != null) {
                        values["controller"] = "Rss";
                        values["action"] = "Get";
                        values["userSlug"] = candidate.AppUser.Slug;
                        values["podcastSlug"] = candidate.Slug;
                    }
                } catch (Exception ex) {
                    _logger.LogError("Error redirecting custom domain");
                    _logger.LogError(ex.Message);
                }
            }
            return values;
        }
    }
}
