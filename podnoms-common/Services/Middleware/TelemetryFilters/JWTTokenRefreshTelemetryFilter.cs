using System;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace PodNoms.Common.Services.Middleware.TelemetryFilters;

//filter out all the 401s from JWT token refreshes
public class JWTTokenRefreshTelemetryFilter : ITelemetryProcessor {
  public JWTTokenRefreshTelemetryFilter(ITelemetryProcessor next) {
    Next = next;
  }

  private ITelemetryProcessor Next { get; }

  public void Process(ITelemetry telemetry) {
    var request = telemetry as RequestTelemetry;
    if (request != null &&
        request.ResponseCode.Equals("401", StringComparison.OrdinalIgnoreCase)) {
      return;
    }

    // Send everything else: 
    Next.Process(telemetry);
  }
}
