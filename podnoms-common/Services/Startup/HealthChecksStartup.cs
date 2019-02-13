using System;
using System.Linq;
using System.Net.Mime;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

using Newtonsoft.Json;
namespace PodNoms.Common.Services.Startup {
    public static class HealthChecksStartup {

        public static IServiceCollection AddPodNomsHealthChecks(this IServiceCollection services, 
                        IConfiguration Configuration) {
            services.AddHealthChecks()
                .AddSqlServer(
                    connectionString: Configuration["ConnectionStrings:DefaultConnection"],
                    healthQuery: "SELECT 1;",
                    name: "DATABASE",
                    failureStatus: HealthStatus.Degraded,
                    tags: new string[] { "db", "sql", "sqlserver" })
                .AddUrlGroup(new Uri("https://www.podnoms.com"), "WWW", HealthStatus.Unhealthy)
                .AddApplicationInsightsPublisher();

            return services;
        }

        public static IApplicationBuilder UsePodNomsHealthChecks(
                        this IApplicationBuilder app, PathString path) {
            // builder.UseHealthChecks(path, new HealthCheckOptions
            // {
            //     ResponseWriter = async (context, report) => {
            //         var result = JsonConvert.SerializeObject(
            //             new {
            //                 status = report.Status.ToString(),
            //                 errors = report.Entries.Select(e => new {
            //                     key = e.Key,
            //                     value = Enum.GetName(typeof(HealthStatus), e.Value.Status)
            //                 })
            //             });
            //         context.Response.ContentType = MediaTypeNames.Application.Json;
            //         await context.Response.WriteAsync(result);
            //     }
            // });
            app.UseHealthChecks("/hc", new HealthCheckOptions()
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });

            app.UseHealthChecksUI(setup => { setup.ApiPath = "/hc"; setup.UIPath = "/healthcheckui";});

            return app;
        }
    }
}