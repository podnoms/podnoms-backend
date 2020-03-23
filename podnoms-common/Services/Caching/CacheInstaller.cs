using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PodNoms.Common.Services.Caching {
    public static class CacheInstaller {
        public static IServiceCollection AddPodNomsCacheService(
            this IServiceCollection services,
            IConfiguration configuration, bool useLocalConnection) {
            var redisCacheSettings = new RedisCacheSettings();
            configuration.GetSection(nameof(RedisCacheSettings)).Bind(redisCacheSettings);
            services.AddSingleton<RedisCacheSettings>(redisCacheSettings);

            if (redisCacheSettings.Enabled) {
                var connection = useLocalConnection ?
                    redisCacheSettings.LocalConnectionString :
                    redisCacheSettings.ConnectionString;
                Console.WriteLine($"Starting redis: {connection}");
                services.AddStackExchangeRedisCache(options =>
                    options.Configuration = connection);
                services.AddSingleton<IResponseCacheService, ResponseCacheService>();
            }

            return services;
        }
    }
}
