using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Extensibility.Implementation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PodNoms.Common.Services.Middleware.TelemetryFilters;

namespace PodNoms.Common.Services.Startup {
    public static class LoggingStartup {
        public static IServiceCollection AddPodNomsAppInsights(
                this IServiceCollection services,
                IConfiguration Configuration, bool isProduction) {

            if (isProduction) {
                services.AddApplicationInsightsTelemetryProcessor<SignalRTelemetryFilter>();
                services.AddApplicationInsightsTelemetryProcessor<RSSFeedTelemetryFilter>();
                services.AddApplicationInsightsTelemetryProcessor<JWTTokenRefreshTelemetryFilter>();
                services.AddApplicationInsightsTelemetry();
            } else {
                var telemetryConfiguration = TelemetryConfiguration.CreateDefault();
                telemetryConfiguration.DisableTelemetry = true;

                var telemetryClient = new TelemetryClient(telemetryConfiguration);   // Use this instance
                TelemetryDebugWriter.IsTracingDisabled = true;
            }
            return services;
        }
    }
}
