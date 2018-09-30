using System;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PodNoms.Common.Auth;
using PodNoms.Common.Services.Processor;

namespace PodNoms.Common.Services.Startup {
    public static class HangfireStartup {
        public static IServiceCollection AddPodNomsHangfire(this IServiceCollection services, IConfiguration config) {
//            System.Console.WriteLine(config.GetConnectionString("DefaultConnection"));
            services.AddHangfire(options => {
                options.UseSqlServerStorage(config.GetConnectionString("DefaultConnection"));
            });
            return services;
        }

        public static IApplicationBuilder UsePodNomsHangfire(
            this IApplicationBuilder builder, IServiceProvider serviceProvider, IConfiguration config) {
//            System.Console.WriteLine(config.GetConnectionString("DefaultConnection"));
            builder.UseHangfireServer()
                .UseHangfireDashboard("/hangfire", new DashboardOptions {
                    Authorization = new[] {new HangFireAuthorizationFilter()}
                });
            GlobalConfiguration.Configuration.UseSqlServerStorage(config.GetConnectionString("DefaultConnection"));
            GlobalConfiguration.Configuration.UseActivator(new HangfireActivator(serviceProvider));

            return builder;
        }
    }
}