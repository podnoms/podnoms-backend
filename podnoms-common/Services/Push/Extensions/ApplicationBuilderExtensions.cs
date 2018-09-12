using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using PodNoms.Api.Services.Push.Data;

namespace PodNoms.Api.Services.Push.Extensions {
    public static class ApplicationBuilderExtensions {
        public static IApplicationBuilder UseSqlitePushSubscriptionStore(this IApplicationBuilder app,
            ServiceProvider services) {
            PushSubscriptionContext context = services.GetService<PushSubscriptionContext>();
            context.Database.EnsureCreated();

            return app;
        }
    }
}