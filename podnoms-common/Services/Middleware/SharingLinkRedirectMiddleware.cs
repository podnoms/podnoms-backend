using System.Threading.Tasks;
using Flurl;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PodNoms.Common.Data.Settings;
using PodNoms.Common.Persistence.Repositories;

namespace PodNoms.Common.Services.Middleware {
    public sealed class SharingLinkRedirectMiddleware {
        private readonly RequestDelegate _next;
        private readonly AppSettings _appSettings;
        private readonly SharingSettings _sharingSettings;
        private readonly ILogger<CustomDomainRedirectMiddleware> _logger;

        public SharingLinkRedirectMiddleware(RequestDelegate next,
            IOptions<AppSettings> appSettings,
            IOptions<SharingSettings> sharingSettings,
            ILogger<CustomDomainRedirectMiddleware> logger) {
            _next = next;
            _appSettings = appSettings.Value;
            _sharingSettings = sharingSettings.Value;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context, IEntryRepository entryRepository) {
            var requestHost = context.Request.Host.Host;
            var baseHost = new System.Uri(_sharingSettings.BaseUrl).Host;
            if (requestHost.Equals(baseHost)) {
                var requestPath = context.Request.Path.Value.TrimStart('/').TrimEnd('/');
                if (!string.IsNullOrEmpty(requestPath)) {
                    var entryId = await entryRepository.GetIdForShareLink(requestPath);
                    if (!string.IsNullOrEmpty(entryId)) {
                        var redirectUrl = Url.Combine(_appSettings.SiteUrl, "sharing", "entry", requestPath);
                        context.Response.Redirect(redirectUrl);
                        return;
                    }
                } else {
                    context.Response.Redirect(Url.Combine(_appSettings.SiteUrl, "sharing/notfound"));
                    return;
                }
            }

            await _next(context);
        }
    }
}