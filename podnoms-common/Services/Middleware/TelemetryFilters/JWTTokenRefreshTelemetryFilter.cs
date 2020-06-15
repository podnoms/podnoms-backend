using System;
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
            var request = telemetry as RequestTelemetry;
            if (request != null && 
                request.ResponseCode.Equals("401", StringComparison.OrdinalIgnoreCase)){
                return;
            }
            // Send everything else: 
            this.Next.Process(telemetry);
        }
    }
}
