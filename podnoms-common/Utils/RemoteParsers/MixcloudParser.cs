using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace PodNoms.Common.Utils.RemoteParsers {
    public class MixcloudParser {
        static string[] VALID_PATHS = new string[] {
            "stream", "uploads", "favorites", "listens", "playlists"
        };

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<MixcloudParser> _logger;

        public MixcloudParser(IHttpClientFactory httpClientFactory, ILogger<MixcloudParser> logger) {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }
        public static bool ValidateUrl(string url) {
            try {
                var uri = new Uri(url);
                if (uri.Host.EndsWith("mixcloud.com")) {
                    var path = uri.Segments[uri.Segments.Length - 1].ToString().TrimEnd(new[] { '/' });
                    return (VALID_PATHS.Any(path.Equals)) ||
                            uri.Segments.Where(s => s != "/").Count() == 1;
                }
            } catch (Exception) {
            }
            return false;
        }
        public async Task<List<ParsedItemResult>> GetEntries(string url, int count = 10) {
            try {
                var path = new Uri(url).Segments.First(s => s != "/");
                var newUrl = HttpUtils.UrlCombine(path, "cloudcasts");
                var client = _httpClientFactory.CreateClient("mixcloud");
                var result = await client.GetAsync(newUrl);

                if (result.IsSuccessStatusCode) {
                    var body = await result.Content.ReadAsStringAsync();
                    var typed = JsonConvert.DeserializeObject<Welcome>(body, MixcloudJsonConverter.Settings);
                    var data = typed.Data.OrderByDescending(p => p.UpdatedTime)
                        .Select(c => new ParsedItemResult {
                            Id = c.Key,
                            Title = c.Name,
                            VideoType = "mixcloud",
                            UploadDate = c.UpdatedTime.DateTime
                        }).Take(10).ToList();
                    return data;
                }
            } catch (Exception ex) {
                _logger.LogError($"Error parsing url: {url}");
                _logger.LogError(ex.Message);
            }
            return null;
        }
    }
}
