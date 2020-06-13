using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace PodNoms.Common.Services.Middleware.TelemetryFilters {


    //filter out all the 401s from JWT token refreshes
    public class JWTTokenRefreshTelemetryFilter : ITelemetryProcessor {
        private ITelemetryProcessor Next { get; set; }

        public JWTTokenRefreshTelemetryFilter(ITelemetryProcessor next) {
            this.Next = next;
        }

        public void Process(ITelemetry telemetry) {
            if (telemetry is RequestTelemetry && ((RequestTelemetry)telemetry).ResponseCode == "401") {
                return;
            }
            this.Next.Process(telemetry);
        }

    }
}
