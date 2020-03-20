using System;
using System.Threading.Tasks;

namespace PodNoms.Common.Services.Caching {
    public interface IResponseCacheService {
        Task CacheResponseAsync(string key, string response, TimeSpan ttl);
        Task<string> GetCacheResponseAsync(string key);
        Task InvalidateCacheResponseAsync(string key);
    }
}
