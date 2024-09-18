using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace PodNoms.Common.Utils.Extensions;

public static class HttpClientExtensions {
  public static async Task<long> GetContentSizeAsync(this HttpClient client, string url,
    ILogger logger = null) {
    try {
      using var request = new HttpRequestMessage(HttpMethod.Get, url);
      // In order to keep the response as small as possible, set the requested byte range to [0,0] (i.e., only the first byte)
      request.Headers.Range = new RangeHeaderValue(0, 0);

      using var response = await client.SendAsync(request);
      response.EnsureSuccessStatusCode();

      if (response.StatusCode != HttpStatusCode.PartialContent) {
        throw new WebException(
          $@"expected partial content response\n
                            ({HttpStatusCode.PartialContent}),\n
                            instead received: {response.StatusCode}"
        );
      }

      var contentRange = response.Content.Headers.GetValues(@"Content-Range").Single();
      var lengthString = Regex.Match(
        contentRange,
        @"(?<=^bytes\s[0-9]+\-[0-9]+/)[0-9]+$").Value;
      return long.Parse(lengthString);
    } catch (Exception ex) {
      logger?.LogError(ex.Message);
    }

    return -1;
  }
}
