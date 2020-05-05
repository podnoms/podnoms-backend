using System;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using EasyNetQ.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PodNoms.Data.Enums;

namespace PodNoms.Common.Services.Caching {
    public class CachedAttribute : Attribute, IAsyncActionFilter {
        private readonly int _ttl;
        private readonly string _itemType;
        private readonly CacheType _cacheType;
        private readonly string _contentType;

        public CachedAttribute(string itemType, CacheType cacheType, string contentType = "application/json", int ttl = 60) {
            _itemType = itemType;
            _cacheType = cacheType;
            _contentType = contentType;
            _ttl = ttl;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next) {
            var cacheSettings = context.HttpContext.RequestServices.GetRequiredService<RedisCacheSettings>();
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<CachedAttribute>>();

            if (!cacheSettings.Enabled) {
                await next();
                return;
            }

            var cache = context.HttpContext.RequestServices.GetRequiredService<IResponseCacheService>();
            var key = _getCacheKey(_itemType, _cacheType.ToString().ToLower(), context.HttpContext.Request);

            var cachedResponse = await cache.GetCacheResponseAsync(key);
            if (!string.IsNullOrEmpty(cachedResponse)) {
                logger.LogDebug($"Cache hit: {key}");
                var contentResult = new ContentResult {
                    ContentType = _contentType,
                    StatusCode = 200,
                    Content = cachedResponse
                };
                context.Result = contentResult;
                return;
            }

            logger.LogDebug($"Cache miss: {key}");
            var executedContext = await next();
            if (executedContext.Result is ContentResult result) {
                await cache.CacheResponseAsync(key, result.Content, TimeSpan.FromSeconds(_ttl));
            }
        }

        private static string _getCacheKey(string itemType, string cacheType, HttpRequest request) {
            var keyBuilder = new StringBuilder();
            keyBuilder.Append($"{itemType}");
            var values = request.Path.ToString().Split("/")
                .Where(x => !string.IsNullOrEmpty(x))
                .Where(x => !x.Equals(cacheType));
            foreach (var value in values) {
                keyBuilder.Append($"|{value}");
            }
            keyBuilder.Append($"|{cacheType}");
            return keyBuilder.ToString();
        }
    }
}
