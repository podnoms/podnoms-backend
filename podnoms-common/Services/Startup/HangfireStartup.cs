using System;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PodNoms.Api.Services.Auth;
using PodNoms.Api.Services.Processor;

namespace PodNoms.Services.Services.Startup {
    public static class HangfireStartup {
        public static IServiceCollection AddPodNomsHangfire(this IServiceCollection services, IConfiguration config) {
            services.AddHangfire(options => {
                options.UseSqlServerStorage(config.GetConnectionString("DefaultConnection"));
            });
            return services;
        }

        public static IApplicationBuilder UsePodNomsHangfire(
            this IApplicationBuilder builder, IServiceProvider serviceProvider, IConfiguration config) {
            builder.UseHangfireDashboard("/hangfire", new DashboardOptions {
                Authorization = new[] {new HangFireAuthorizationFilter()}
            });
            GlobalConfiguration.Configuration.UseSqlServerStorage(config.GetConnectionString("DefaultConnection"));
            GlobalConfiguration.Configuration.UseActivator(new HangfireActivator(serviceProvider));

            return builder;
        }
    }
}