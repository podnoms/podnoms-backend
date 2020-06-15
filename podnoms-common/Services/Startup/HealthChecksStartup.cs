using System;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace PodNoms.Common.Services.Startup {
    public static class HealthChecksStartup {
        private const bool ENABLED = true;

        public static IServiceCollection AddPodNomsHealthChecks(
            this IServiceCollection services,
            IConfiguration Configuration, bool isDevelopment) {
            if (isDevelopment || !ENABLED) {
                return services;
            }

            // disable UI in dev as self-signed certs aren't allowed
            // services.AddHealthChecksUI();
            services.AddHealthChecks()
                .AddSqlServer(
                    connectionString: Configuration["ConnectionStrings:DefaultConnection"],
                    healthQuery: "SELECT 1;",
                    name: "DATABASE",
                    failureStatus: HealthStatus.Unhealthy,
                    tags: new string[] {"db", "sql", "sqlserver"})
                .AddUrlGroup(
                    new Uri(Configuration["AppSettings:JobServerUrl"]),
                    name: "jobserver",
                    failureStatus: HealthStatus.Degraded)
                .AddAzureBlobStorage(
                    Configuration["StorageSettings:ConnectionString"],
                    name: "CDN",
                    failureStatus: HealthStatus.Degraded)
                .AddHangfire(s => {
                    s.MaximumJobsFailed = 5;
                    s.MinimumAvailableServers = 1;
                }, name: "Hangfire", failureStatus: HealthStatus.Degraded);
            // .AddRabbitMQ(
            //     $"amqp://{Configuration["RabbitMq:ConnectionString"]}",
            //     name: "BROKER",
            //     failureStatus: HealthStatus.Degraded,
            //     tags: new string[] { "messages", "broker", "queue", "messagequeue" });

            return services;
        }

        public static IApplicationBuilder UsePodNomsHealthChecks(this IApplicationBuilder app, bool isDevelopment) {
            if (isDevelopment || !ENABLED) {
                return app;
            }

            app.UseHealthChecks("/hc-api", new HealthCheckOptions {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });
            return app;
        }
    }
}
