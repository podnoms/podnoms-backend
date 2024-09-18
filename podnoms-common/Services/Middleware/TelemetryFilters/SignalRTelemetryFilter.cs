using System;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace PodNoms.Common.Services.Middleware.TelemetryFilters;

public class SignalRTelemetryFilter : ITelemetryProcessor {
  public SignalRTelemetryFilter(ITelemetryProcessor next) {
    Next = next;
  }

  private ITelemetryProcessor Next { get; }

  public void Process(ITelemetry item) {
    if (item is RequestTelemetry request &&
        request.Url.AbsolutePath.IndexOf(
          "/hubs/", StringComparison.InvariantCultureIgnoreCase) >= 0) {
      return;
    }

    Next.Process(item);
  }
}
