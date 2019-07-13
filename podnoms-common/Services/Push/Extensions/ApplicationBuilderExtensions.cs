using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PodNoms.Common.Services.Push.Data;

namespace PodNoms.Common.Services.Push.Extensions {
    public static class ApplicationBuilderExtensions {
        public static IApplicationBuilder UseSqlitePushSubscriptionStore(this IApplicationBuilder app) {
            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope()) {
                try {
                    var context = serviceScope.ServiceProvider.GetService<PushSubscriptionContext>();
                    context.Database.EnsureCreated();
                    context.Database.Migrate();
                } catch (Exception ex) {
                    System.Console.Error.WriteLine($"Error configuring push\n{ex.Message}");
                }
            }
            return app;
        }
    }
}
