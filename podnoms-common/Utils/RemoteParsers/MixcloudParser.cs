using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace PodNoms.Common.Utils.RemoteParsers {
    public class MixcloudParser {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<MixcloudParser> _logger;

        public MixcloudParser(IHttpClientFactory httpClientFactory, ILogger<MixcloudParser> logger) {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        /// <summary>
        /// Load results from Mixcloud using an offset and size
        /// </summary>
        /// <param name="url"></param>
        /// <param name="offset"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        private async Task<MixcloudApiResult> _getEntries(string url, int offset = 0, int take = 20) {
            var path = new Uri(url).Segments.First(s => s != "/");
            var newUrl = HttpUtils.UrlCombine(path, $"cloudcasts?offset={offset}");
            var client = _httpClientFactory.CreateClient("mixcloud");
            var result = await client.GetAsync(newUrl);
            if (!result.IsSuccessStatusCode) {
                return null;
            }

            var body = await result.Content.ReadAsStringAsync();
            var typed = JsonConvert.DeserializeObject<MixcloudApiResult>(body, MixcloudJsonConverter.Settings);

            return typed;
        }

        /// <summary>
        /// API to get all entries for a playlist
        /// The Mixcloud API doesn't allow all entries in a single call
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public async Task<List<ParsedItemResult>> GetAllEntries(string url) {
            List<ParsedItemResult> results = new();
            int offset = 1;
            var currentEntries = await _getEntries(url, offset - 1);
            while (currentEntries is not null && currentEntries.Shows.Length != 0) {
                results.AddRange(_toParsedResponse(currentEntries));
                offset += currentEntries.Shows.Length;
                currentEntries = await _getEntries(
                    url,
                    offset - 1,
                    currentEntries.Shows.Length /*Length of shows is probably a decent guess at the API return limit*/);
            }

            return results
                .OrderBy(r => r.UploadDate)
                .ToList();
        }

        private static IEnumerable<ParsedItemResult> _toParsedResponse(MixcloudApiResult typed) {
            var data = typed?.Shows.OrderByDescending(p => p.UpdatedTime)
                .Select(c => new ParsedItemResult {
                    Id = c.Key,
                    Title = c.Name,
                    VideoType = "mixcloud",
                    UploadDate = c.UpdatedTime.DateTime
                });
            return data;
        }

        //TODO: Either refactor this to use the ^^ above or remove altogether
        public async Task<List<ParsedItemResult>> GetEntries(string url, int take = 10) {
            try {
                var path = new Uri(url).Segments.First(s => s != "/");
                var newUrl = HttpUtils.UrlCombine(path, "cloudcasts");
                var client = _httpClientFactory.CreateClient("mixcloud");
                var result = await client.GetAsync(newUrl);

                if (result.IsSuccessStatusCode) {
                    var body = await result.Content.ReadAsStringAsync();
                    var typed = JsonConvert.DeserializeObject<MixcloudApiResult>(body, MixcloudJsonConverter.Settings);

                    var data = typed?.Shows.OrderByDescending(p => p.UpdatedTime)
                        .Select(c => new ParsedItemResult {
                            Id = c.Key,
                            Title = c.Name,
                            VideoType = "mixcloud",
                            UploadDate = c.UpdatedTime.DateTime
                        }).Take(take).ToList();
                    return data;
                }
            } catch (Exception ex) {
                _logger.LogError("Error parsing url: {Url}", url);
                _logger.LogError("{Error}", ex.Message);
            }

            return null;
        }
    }
}
