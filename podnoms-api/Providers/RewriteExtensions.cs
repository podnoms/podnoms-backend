using System;
using Flurl;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Configuration;
using PodNoms.Common.Data.Settings;

namespace PodNoms.Api.Providers {
    public static class RewriteExtensions {
        public static IApplicationBuilder UseCustomDomainRewrites(this IApplicationBuilder app) {

            var options = new RewriteOptions()
                .Add(MethodRules.RedirectShortUrlHost)
                .Add(MethodRules.RedirectCustomDomain)
                .Add(MethodRules.RedirectRssFeed);

            app.UseRewriter(options);
            return app;
        }
    }
    static class MethodRules {
        public static void RedirectShortUrlHost(RewriteContext context) {
            var request = context.HttpContext.Request;
            var config = Startup.Configuration
                .GetSection("SharingSettings")
                .Get<SharingSettings>();

            var requestPath = request.Path.Value.TrimStart('/').TrimEnd('/')
                .Split('/');

            //check it's a single item path and doesn't have an extension
            //this ensures we can still serve from wwwroot
            if (requestPath.Length == 1 && !requestPath[0].Contains(".")) {
                if (request.Host.Value.Equals(new Uri(config.BaseUrl).Authority, StringComparison.OrdinalIgnoreCase)) {
                    context.Result = RuleResult.SkipRemainingRules;
                    request.Path = $"/pub/sharing/{requestPath[0]}";
                }
            }
        }
        public static void RedirectCustomDomain(RewriteContext context) {

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
