using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PodNoms.Common.Data.Settings;
using PodNoms.Common.Persistence.Repositories;

namespace PodNoms.Common.Services.Middleware {
    public sealed class HostNameRouter : IRouter {
        private readonly IRouter _defaultRouter;
        private readonly IConfigurationSection _sharingSettings;

        public HostNameRouter(IRouter defaultRouteHandler,
                            IConfigurationSection sharingSettings) {
            _defaultRouter = defaultRouteHandler;
            _sharingSettings = sharingSettings;
        }
        public VirtualPathData GetVirtualPath(VirtualPathContext context) {
            return _defaultRouter.GetVirtualPath(context);
        }

        public async Task RouteAsync(RouteContext context) {
            var path = context.HttpContext.Request.Path.Value.Split('/');
            var currentHost = context.HttpContext.Request.Host.ToString();
            var sharingHost = new System.Uri(_sharingSettings.GetValue<string>("BaseUrl")).Authority;
            if (currentHost.ToLower().Equals(sharingHost.ToLower())) {
                var action = "Index";
                var controller = "TestRouting";

                context.RouteData.Values["controller"] = $"/pub/sharing/{controller}";
                context.RouteData.Values["action"] = action;

                await _defaultRouter.RouteAsync(context);
            }
        }
    }
}
