using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace PodNoms.Common.Services.Caching {
    public class ResponseCacheService : IResponseCacheService {
        private IDistributedCache _cache;
        private ILogger<ResponseCacheService> _logger;

        public ResponseCacheService(IDistributedCache cache, ILogger<ResponseCacheService> logger) {
            _cache = cache;
            _logger = logger;
        }

        public async Task CacheResponseAsync(string key, string response, TimeSpan ttl) {
            if (response == null) {
                return;
            }

            await _cache.SetStringAsync(key, response, new DistributedCacheEntryOptions {
                AbsoluteExpirationRelativeToNow = ttl
            });
        }

        public async Task<string> GetCacheResponseAsync(string key) {
            var cachedResponse = await _cache.GetStringAsync(key);
            return string.IsNullOrEmpty(cachedResponse) ? null : cachedResponse;
        }

        public async Task InvalidateCacheResponseAsync(string key) {
            try {
                if (!string.IsNullOrEmpty(await GetCacheResponseAsync(key))) {
                    await _cache.RemoveAsync(key);
                }
            } catch (Exception e) {
                _logger.LogError($"Unable to invalidate cache: {e.Message}");
            }
        }
    }
}
