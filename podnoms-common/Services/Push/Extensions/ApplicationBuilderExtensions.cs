using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using PodNoms.Common.Services.Push.Data;

namespace PodNoms.Common.Services.Push.Extensions {
    public static class ApplicationBuilderExtensions {
        public static IApplicationBuilder UseSqlitePushSubscriptionStore(this IApplicationBuilder app) {
            System.Console.WriteLine($"In Scope");

            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope()) {
                System.Console.WriteLine($"Starting try");
                try {
                    System.Console.WriteLine($"Getting service context");
                    var context = serviceScope.ServiceProvider.GetService<PushSubscriptionContext>();
                    System.Console.WriteLine($"Ensuring database created");
                    context.Database.EnsureCreated();
                    System.Console.WriteLine($"Database created");
                } catch (Exception ex) {
                    System.Console.WriteLine($"Error configuring push\n{ex.Message}");
                }
                System.Console.WriteLine($"Exiting try block");
            }
            System.Console.WriteLine($"Returning app builder");
            return app;
        }
    }
}