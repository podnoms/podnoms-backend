using Lib.Net.Http.WebPush.Authentication;
using Microsoft.Extensions.DependencyInjection;
using PodNoms.Common.Services.Push.Data;

namespace PodNoms.Common.Services.Push.Extensions {
    public static class PushServiceServiceCollectionExtensions {
        public static IServiceCollection AddPushServicePushNotificationService(this IServiceCollection services) {
            services.AddMemoryCache();
            services.AddSingleton<IVapidTokenCache, MemoryVapidTokenCache>();
            services.AddSingleton<IPushNotificationService, VapidPushNotificationService>();
            return services;
        }
    }
}