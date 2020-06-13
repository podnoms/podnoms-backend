using System;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace PodNoms.Common.Services.Middleware.TelemetryFilters {
    public class SignalRTelemetryFilter : ITelemetryProcessor {
        private ITelemetryProcessor Next { get; set; }

        public SignalRTelemetryFilter(ITelemetryProcessor next) {
            this.Next = next;
        }

        public void Process(ITelemetry item) {
            if (item is RequestTelemetry request &&
                request.Url.AbsolutePath.IndexOf(
                    "/hubs/", StringComparison.InvariantCultureIgnoreCase) >= 0) {
                return;
            }

            this.Next.Process(item);
        }
    }
}
