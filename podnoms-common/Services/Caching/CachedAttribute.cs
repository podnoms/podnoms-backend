using System;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace PodNoms.Common.Services.Caching {
    public class CachedAttribute : Attribute, IAsyncActionFilter {
        private readonly int _ttl;
        private readonly string _itemType;
        private readonly string _contentType;

        public CachedAttribute(string itemType, string contentType = "application/json", int ttl = 60) {
            _itemType = itemType;
            _contentType = contentType;
            _ttl = ttl;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next) {
            var cacheSettings = context.HttpContext.RequestServices.GetRequiredService<RedisCacheSettings>();

            if (!cacheSettings.Enabled) {
                await next();
                return;
            }

            var cache = context.HttpContext.RequestServices.GetRequiredService<IResponseCacheService>();
            var key = _getCacheKey(_itemType, context.HttpContext.Request);

            var cachedResponse = await cache.GetCacheResponseAsync(key);
            if (!string.IsNullOrEmpty(cachedResponse)) {
                var contentResult = new ContentResult {
                    ContentType = _contentType,
                    StatusCode = 200,
                    Content = cachedResponse
                };
                context.Result = contentResult;
                return;
            }

            var executedContext = await next();
            if (executedContext.Result is ContentResult result) {
                await cache.CacheResponseAsync(key, result.Content, TimeSpan.FromSeconds(_ttl));
            }
        }

        private static string _getCacheKey(string type, HttpRequest request) {
            var keyBuilder = new StringBuilder();
            keyBuilder.Append($"{type}");
            foreach (var value in request.Path.ToString().Split("/").OrderBy(x => x)) {
                keyBuilder.Append($"__{value}");
            }

            return keyBuilder.ToString();
        }
    }
}
