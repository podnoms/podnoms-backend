using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PodNoms.Common.Data.Settings;
using PodNoms.Common.Persistence;

namespace PodNoms.Common.Services.Middleware {
    public class SharingLinkRouteTransformer : DynamicRouteValueTransformer {
        private readonly SharingSettings _sharingSettings;
        private readonly IServiceProvider _provider;
        private readonly ILogger _logger;

        public SharingLinkRouteTransformer(
            IOptions<SharingSettings> sharingSettings,
            IServiceProvider provider,
            ILogger<SharingLinkRouteTransformer> logger) {
            _sharingSettings = sharingSettings.Value;
            _provider = provider;
            _logger = logger;
        }

        public override async ValueTask<RouteValueDictionary> TransformAsync(HttpContext context,
            RouteValueDictionary values) {
            var requestHost = context.Request.Host.Host;
            var baseHost = new System.Uri(_sharingSettings.BaseUrl).Host;
            if (!requestHost.Equals(baseHost)) {
                return values;
            }

            var requestPath = context.Request.Path.Value.TrimStart('/').TrimEnd('/');
            if (string.IsNullOrEmpty(requestPath)) {
                return values;
            }

            using var scope = _provider.CreateScope();
            var repoAccessor = scope.ServiceProvider.GetRequiredService<IRepoAccessor>();
            var entryId = await repoAccessor.Entries.GetIdForShareLink(requestPath);
            if (string.IsNullOrEmpty(entryId)) {
                return values;
            }

            _logger.LogDebug($"Matched route {requestPath}");
            values["controller"] = "PublicSharing";
            values["action"] = "Index";
            values["shareId"] = requestPath;
            _logger.LogDebug(
                $"Controller: {values["controller"]}\nAction: {values["action"]}\nShareId: {values["shareId"]}");

            return values;
        }
    }
}
