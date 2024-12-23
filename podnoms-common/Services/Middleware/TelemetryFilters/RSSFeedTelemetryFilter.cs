using System;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Options;
using PodNoms.Common.Data.Settings;

namespace PodNoms.Common.Services.Middleware.TelemetryFilters;

public class RSSFeedTelemetryFilter : ITelemetryProcessor {
  private readonly AppSettings _appSettings;

  public RSSFeedTelemetryFilter(ITelemetryProcessor next, IOptions<AppSettings> appSettings) {
    Next = next;
    _appSettings = appSettings.Value;
  }

  private ITelemetryProcessor Next { get; }

  public void Process(ITelemetry item) {
    if (item is RequestTelemetry request) {
      var requestHost = request.Url;
      var siteHost = new UriBuilder(_appSettings.SiteUrl).Host;
      var cleaned = new Uri(_appSettings.RssUrl).GetComponents(
          UriComponents.AbsoluteUri & ~UriComponents.Port & ~UriComponents.Scheme,
          UriFormat.UriEscaped)
        .TrimEnd('/');
      if (requestHost.Equals(cleaned)) {
        return;
      }

      Next.Process(item);
    }
  }
}
