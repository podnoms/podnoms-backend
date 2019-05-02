using Microsoft.ApplicationInsights.AspNetCore;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Extensibility.PerfCounterCollector.QuickPulse;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PodNoms.Common.Services.Logging {

    public static class ApplicationInsightsProvider {
        public static IApplicationBuilder UsePodNomsApplicationInsights(
            this IApplicationBuilder builder, IConfigurationSection config, bool isProduction) {
            if (isProduction && config.Value != null) {

                var telemetryKey = config.GetValue<string>("InstrumentationKey");
                if (!string.IsNullOrEmpty(telemetryKey)) {
                    var configuration = new TelemetryConfiguration();
                    configuration.InstrumentationKey = telemetryKey;

                    QuickPulseTelemetryProcessor processor = null;

                    configuration.TelemetryProcessorChainBuilder
                        .Use((next) => {
                            processor = new QuickPulseTelemetryProcessor(next);
                            return processor;
                        })
                        .Build();

                    var QuickPulse = new QuickPulseTelemetryModule();
                    QuickPulse.Initialize(configuration);
                    QuickPulse.RegisterTelemetryProcessor(processor);
                }
            }
            return builder;
        }
        public static IServiceCollection AddPodNomsApplicationInsights(this IServiceCollection services,
                IConfiguration Configuration, bool isProduction) {
            if (isProduction) {
                services.AddApplicationInsightsTelemetry(Configuration);
            }
            return services;
        }       
    }
}