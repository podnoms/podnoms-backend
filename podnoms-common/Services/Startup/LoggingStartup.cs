using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PodNoms.Common.Services.Startup {
    public static class LoggingStartup {
        public static IServiceCollection AddPodNomsAppInsights(
                this IServiceCollection services,
                IConfiguration Configuration, bool isProduction) {

            if (!isProduction){
                services.AddApplicationInsightsTelemetry();
            }
            return services;
        }
    }
}
