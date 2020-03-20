using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PodNoms.Common.Services.Caching {
    public static class CacheInstaller {
        public static IServiceCollection AddPodNomsCacheService(
            this IServiceCollection services,
            IConfiguration configuration) {
            var redisCacheSettings = new RedisCacheSettings();
            configuration.GetSection(nameof(RedisCacheSettings)).Bind(redisCacheSettings);
            services.AddSingleton<RedisCacheSettings>(redisCacheSettings);
            if (redisCacheSettings.Enabled) {
                services.AddStackExchangeRedisCache(options =>
                    options.Configuration = redisCacheSettings.ConnectionString);
                services.AddSingleton<IResponseCacheService, ResponseCacheService>();
            }

            return services;
        }
    }
}
