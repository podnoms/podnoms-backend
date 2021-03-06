﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Flurl;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PodNoms.Common.Data.Settings;
using PodNoms.Common.Persistence.Repositories;

namespace PodNoms.Common.Services.Middleware {

    public class SharingLinkRouteTransformer : DynamicRouteValueTransformer {
        private readonly AppSettings _appSettings;
        private readonly SharingSettings _sharingSettings;
        private readonly IServiceProvider _provider;
        private readonly ILogger _logger;

        public SharingLinkRouteTransformer(IOptions<AppSettings> appSettings,
                                    IOptions<SharingSettings> sharingSettings,
                                    IServiceProvider provider,
                                    ILogger<SharingLinkRouteTransformer> logger) {
            _appSettings = appSettings.Value;
            _sharingSettings = sharingSettings.Value;
            _provider = provider;
            _logger = logger;
        }

        public override async ValueTask<RouteValueDictionary> TransformAsync(HttpContext context, RouteValueDictionary values) {
            var requestHost = context.Request.Host.Host;
            var baseHost = new System.Uri(_sharingSettings.BaseUrl).Host;
            if (requestHost.Equals(baseHost)) {
                var requestPath = context.Request.Path.Value.TrimStart('/').TrimEnd('/');
                if (!string.IsNullOrEmpty(requestPath)) {
                    using var scope = _provider.CreateScope();
                    var entryRepository = scope.ServiceProvider.GetRequiredService<IEntryRepository>();

                    var entryId = await entryRepository.GetIdForShareLink(requestPath);
                    if (!string.IsNullOrEmpty(entryId)) {
                        _logger.LogDebug($"Matched route {requestPath}");
                        values["controller"] = "PublicSharing";
                        values["action"] = "Index";
                        values["shareId"] = requestPath;
                        _logger.LogDebug($"Controller: {values["controller"]}\nAction: {values["action"]}\nShareId: {values["shareId"]}");
                    }
                } else {
                    // context.Response.Redirect(Url.Combine(_appSettings.ApiUrl, "sharing/notfound"));
                }
            }
            return values;
        }
    }

}
