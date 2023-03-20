using System;
using Flurl;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Configuration;
using PodNoms.Common.Data.Settings;

namespace PodNoms.Api.Providers {
    public class RedirectCustomRssUrl : IRule {
        public RedirectCustomRssUrl() {
        }

        public void ApplyRule(RewriteContext context) {
            // var request = context.HttpContext.Request;
            // var config = Startup.Configuration
            //     .GetSection("AppSettings")
            //     .Get<AppSettings>();

            // var siteHost = new UriBuilder(config.SiteUrl).Host;
            // var cleaned = new Uri(config.RssUrl).GetComponents(
            //         UriComponents.AbsoluteUri & ~UriComponents.Port & ~UriComponents.Scheme, UriFormat.UriEscaped)
            //         .TrimEnd('/');

            // if (request.Host.Equals(cleaned)) {
            //     var redirectUrl = $"{config.CanonicalRssUrl}{request.Path}";
            //     context.Result = RuleResult.SkipRemainingRules;
            //     request.Path = redirectUrl;
            // } else if (!request.Host.Equals(siteHost)) {
            //     //we're on a custom domain, check for matches
            //     var candidate = _repository.GetAll()
            //         .Include(r => r.AppUser)
            //         .Where(r => r.CustomDomain == request.Host.Host)
            //         .FirstOrDefault();

            //     if (candidate != null) {
            //         var redirectUrl = $"/rss/{candidate.AppUser.Slug}/{candidate.Slug}";
            //         context.Result = RuleResult.SkipRemainingRules;
            //         request.Path = redirectUrl;
            //         return;
            //     }
            // }
        }
    }

    static class MethodRules {
        public static void RedirectShortUrlHost(RewriteContext context) {
            var request = context.HttpContext.Request;
            var config = Startup.Configuration
                .GetSection("SharingSettings")
                .Get<SharingSettings>();

            var requestPath = request.Path.Value?.TrimStart('/').TrimEnd('/')
                .Split('/');

            //check it's a single item path and doesn't have an extension
            //this ensures we can still serve from wwwroot
            if (requestPath.Length != 1 || requestPath[0].Contains(".")) {
                return;
            }

            if (!request.Host.Value.Equals(new Uri(config.BaseUrl).Authority, StringComparison.OrdinalIgnoreCase)) {
                return;
            }

            context.Result = RuleResult.SkipRemainingRules;
            request.Path = $"/pub/sharing/{requestPath[0]}";
        }

        public static void RedirectRssFeed(RewriteContext context) {
            var request = context.HttpContext.Request;
            var config = Startup.Configuration
                .GetSection("AppSettings")
                .Get<AppSettings>();

            var requestHost = request.Host.Host;
            var siteHost = new UriBuilder(config.SiteUrl).Host;
            var cleaned = new Uri(config.RssUrl).GetComponents(
                    UriComponents.AbsoluteUri & ~UriComponents.Port & ~UriComponents.Scheme,
                    UriFormat.UriEscaped)
                .TrimEnd('/');

            if (requestHost.Equals(cleaned)) {
                var redirectUrl = Url.Combine("/rss", request.Path);
                context.Result = RuleResult.SkipRemainingRules;
                request.Path = redirectUrl;
            }
        }
    }
}
