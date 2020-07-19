using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PodNoms.Common.Services.Startup {
    public static class SessionStartup {
        public static IServiceCollection AddPodNomsSessionState(
            this IServiceCollection services,
            IConfiguration config,
            bool isProduction) {

            services.AddDistributedMemoryCache();

            services.AddSession(options => {
                options.Cookie.Name = ".AdventureWorks.Session";
                options.IdleTimeout = TimeSpan.FromSeconds(10);

                options.IdleTimeout = TimeSpan.FromSeconds(10);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });
            return services;
        }

        public static IApplicationBuilder UsePodNomsSessionState(
            this IApplicationBuilder builder) {
            builder.UseSession();
            return builder;
        }
    }
}
