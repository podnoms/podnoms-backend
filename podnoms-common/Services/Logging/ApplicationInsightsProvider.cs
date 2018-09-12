using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Extensibility.PerfCounterCollector.QuickPulse;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace PodNoms.Api.Services.Logging {
    public static class ApplicationInsightsProvider {
        public static IApplicationBuilder UsePodNomsApplicationInsights(
            this IApplicationBuilder builder, bool isProduction, IConfigurationSection config) {

            string telemetryKey = config.GetValue<string>("InstrumentationKey");
            if (!string.IsNullOrEmpty(telemetryKey)) {
                TelemetryConfiguration configuration = new TelemetryConfiguration();
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
            return builder;
        }
    }
}