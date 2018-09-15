using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using PodNoms.Common.Services.Push.Data;

namespace PodNoms.Common.Services.Push.Extensions {
    public static class ApplicationBuilderExtensions {
        public static IApplicationBuilder UseSqlitePushSubscriptionStore(this IApplicationBuilder app,
            ServiceProvider services) {
            var context = services.GetService<PushSubscriptionContext>();
            context.Database.EnsureCreated();

            return app;
        }
    }
}