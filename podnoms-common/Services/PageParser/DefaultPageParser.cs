using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;
using PodNoms.Common.Utils.Extensions;

namespace PodNoms.Common.Services.PageParser {
    public class ExternalPageParser : IPageParser {
        private class ExternalPageParserResult {
            public string Result { get; set; }
            public string[] Data { get; set; }
        }

        private HttpClient _httpClient;
        private string _url;

        public ExternalPageParser(IHttpClientFactory httpClientFactory) {
            this._httpClient = httpClientFactory.CreateClient("RemotePageParser");
        }

        public async Task<bool> Initialise(string url) {
            return await Task.Run(() => {
                this._url = url;
                return true;
            });
        }

        public async Task<string> GetPageTitle() {
            var result = await _httpClient.GetFromJsonAsync<ExternalPageParserResult>($"get-page-title?url={_url}");
            return result != null ? result.Data.FirstOrDefault() : string.Empty;
        }

        public async Task<string[]> GetHeadTags() {
            var result = await _httpClient.GetFromJsonAsync<ExternalPageParserResult>($"get-head-meta-tags?url={_url}");
            return result?.Data;
        }

        public async Task<string> GetHeadTag(string tagName) {
            var result = await GetHeadTags();
            return result != null ? result.FirstOrDefault() : string.Empty;
        }

        public async Task<Dictionary<string, string>> GetAllAudioLinks(bool isDeep = false) {
            var parser = isDeep ? "deep-check-url" : "shallow-check-url";
            var result = await _httpClient.GetFromJsonAsync<ExternalPageParserResult>($"{parser}?url={_url}");
            return result?.Data
                .ToDictionary(o => o.GetFilenameFromUrl(), o => o);
        }
    }

    // public class DefaultPageParser : IPageParser {
    //     private const string PARSER_REGEX =
    //         @"((https?|ftp|gopher|telnet|file|notes|ms-help):((//)|(\\\\))+[\w\d:#@%/;$()~_?\+-=\\\.&]*\.mp3)";
    //
    //     private HtmlDocument _doc;
    //     private string _url;
    //
    //     private static async Task<DefaultPageParser> __create(string url) {
    //         var cls = new DefaultPageParser();
    //         await cls.Initialise(url);
    //         return cls;
    //     }
    //
    //     public bool _validateUrl(string url) {
    //         if (Uri.TryCreate(url, UriKind.Absolute, out var validatedUri)) {
    //             return (validatedUri.Scheme == Uri.UriSchemeHttp || validatedUri.Scheme == Uri.UriSchemeHttps);
    //         }
    //
    //         return false;
    //     }
    //
    //     public async Task<bool> Initialise(string url) {
    //         if (_validateUrl(url)) {
    //             this._url = url;
    //             HtmlWeb web = new HtmlWeb();
    //             _doc = await web.LoadFromWebAsync(url);
    //             return true;
    //         }
    //
    //         return false;
    //     }
    //
    //     public async Task<string[]> GetHeadTags() {
    //         return await Task.Run(() => _doc.DocumentNode
    //             .SelectNodes($"//meta")?
    //             .Where(r => r.Attributes["content"].Value is not null)
    //             .Select(r => r.Attributes["content"].Value.ToString())
    //             .ToArray()
    //         );
    //     }
    //
    //     public async Task<string> GetHeadTag(string tagName) {
    //         return await Task.Run(() => _doc.DocumentNode
    //             .SelectSingleNode($"//meta[@property='{tagName}']")?
    //             .Attributes["content"].Value ?? string.Empty);
    //     }
    //
    //     public async Task<string> GetPageTitle() {
    //         return await Task.Run(() => {
    //             if (_doc == null) {
    //                 throw new InvalidOperationException("Initialise must be called first");
    //             }
    //
    //             return _doc.DocumentNode.SelectSingleNode("//title").InnerText;
    //         });
    //     }
    //
    //     public async Task<string> GetPageText() {
    //         return await Task.Run(() => {
    //             if (_doc == null) {
    //                 throw new InvalidOperationException("Initialise must be called first");
    //             }
    //
    //             return _doc.Text;
    //         });
    //     }
    //
    //     public async Task<Dictionary<string, string>> GetAllAudioLinks() {
    //         var documentLinks = (GetAudioLinks());
    //         var iframeLinks = (await GetIFrameLinks())?.Select(r => new {
    //             Key = string.IsNullOrWhiteSpace(r.Key) ? r.Value.GetFilenameFromUrl() : r.Key,
    //             Value = r.Value
    //         }).ToDictionary(x => x.Key, x => x.Value);
    //         var textLinks = _getTextLinks(await GetPageText());
    //
    //         var setA = documentLinks
    //             .GroupBy(p => p.Key, StringComparer.OrdinalIgnoreCase)
    //             .ToDictionary(g => g.Key, g => g.First().Value.ToString(), StringComparer.OrdinalIgnoreCase);
    //         var setB = iframeLinks?
    //             .GroupBy(p => p.Key, StringComparer.OrdinalIgnoreCase)
    //             .ToDictionary(g => g.Key, g => g.First().Value.ToString(), StringComparer.OrdinalIgnoreCase);
    //         var setC = textLinks
    //             .GroupBy(p => p.Key, StringComparer.OrdinalIgnoreCase)
    //             .ToDictionary(g => g.Key, g => g.First().Value.ToString(), StringComparer.OrdinalIgnoreCase);
    //
    //         return setA
    //             .Concat(setB)
    //             .Concat(setC)
    //             .GroupBy(e => e.Key)
    //             .ToDictionary(g => g.Key, g => g.First().Value);
    //     }
    //
    //     private Dictionary<string, string> _getTextLinks(string text) {
    //         var matches = Regex.Matches(text, PARSER_REGEX)
    //             .Cast<Match>()
    //             .Select(m => m.Value)
    //             .Distinct()
    //             .ToArray()
    //             .Select(r => new {
    //                 Key = r,
    //                 Value = r.GetFilenameFromUrl()
    //             }).ToDictionary(x => x.Key, x => x.Value);
    //         return matches;
    //     }
    //
    //     public async Task<Dictionary<string, string>> GetIFrameLinks() {
    //         if (_doc == null) {
    //             throw new InvalidOperationException("Initialise must be called first");
    //         }
    //
    //         var iframes = _doc.DocumentNode.Descendants("iframe")
    //             .Where(r => !string.IsNullOrEmpty(r.Attributes["src"]?.Value.ToString()))
    //             .Where(r => r.Attributes["src"].Value.ToString().StartsWith("http"))
    //             .Select(r => r.Attributes["src"].Value.ToString());
    //
    //         if (iframes?.Any() == true) {
    //             var response = await Task.WhenAll(
    //                 iframes
    //                     .Select(async e => (await DefaultPageParser.__create(e)).GetAudioLinks())
    //             ).ConfigureAwait(false);
    //             return response
    //                 .SelectMany(r => r)
    //                 .ToDictionary(x => x.Key, x => x.Value);
    //         }
    //
    //         return new Dictionary<string, string>();
    //     }
    //
    //     public Dictionary<string, string> GetAudioLinks() {
    //         if (_doc == null) {
    //             throw new InvalidOperationException("Initialise must be called first");
    //         }
    //
    //         HtmlWeb web = new HtmlWeb();
    //         var hrefSources = _doc.DocumentNode.Descendants("a")
    //             .Where(a => (!string.IsNullOrEmpty(a.Attributes["href"]?.Value) && (
    //                 a.Attributes["href"].Value.EndsWith("mp3") ||
    //                 a.Attributes["href"].Value.EndsWith("ogg") ||
    //                 a.Attributes["href"].Value.EndsWith("wav") ||
    //                 a.Attributes["href"].Value.EndsWith("m4a")
    //             )))
    //             .Distinct()
    //             .Select(d => _createItem(d.Attributes["href"].Value));
    //
    //         var audioSources = _doc.DocumentNode.Descendants("audio")
    //             .Where(n => n.Attributes["src"] != null)
    //             .Select(d => _createItem(d.Attributes["src"].Value));
    //
    //         var embeddedAudioSources = _doc.DocumentNode.Descendants("audio")
    //             .Where(n => n.HasChildNodes)
    //             .SelectMany(r => r.ChildNodes.Where(n =>
    //                 n.Attributes["src"] != null &&
    //                 n.Attributes["type"] != null && (
    //                     n.Attributes["type"].Value == "audio/mp3" ||
    //                     n.Attributes["type"].Value == "audio/mpeg" ||
    //                     n.Attributes["type"].Value == "audio/ogg" ||
    //                     n.Attributes["type"].Value == "audio/wav" ||
    //                     n.Attributes["type"].Value == "audio/m4a"
    //                 )
    //             ))
    //             .Select(d => _createItem(d.Attributes["src"].Value));
    //
    //         var results = hrefSources?
    //             .GroupBy(p => p.Key, StringComparer.OrdinalIgnoreCase)
    //             .ToDictionary(g => g.Key, g => g.First().Value.ToString(), StringComparer.OrdinalIgnoreCase)
    //             .Union(embeddedAudioSources?.GroupBy(p => p.Key, StringComparer.OrdinalIgnoreCase)
    //                 .ToDictionary(g => g.Key, g => g.First().Value.ToString(), StringComparer.OrdinalIgnoreCase)
    //             ).Union(
    //                 audioSources?.GroupBy(p => p.Key, StringComparer.OrdinalIgnoreCase)
    //                     .ToDictionary(g => g.Key, g => g.First().Value.ToString(), StringComparer.OrdinalIgnoreCase)
    //             ).ToDictionary(r => r.Key, r => r.Value);
    //         return results ?? new Dictionary<string, string>();
    //     }
    //
    //     private KeyValuePair<string, string> _createItem(string url) {
    //         return new KeyValuePair<string, string>(
    //             _cleanUrl(_url, url),
    //             Path.GetFileName(url)
    //         );
    //     }
    //
    //     private string _cleanUrl(string baseUrl, string remoteUrl) {
    //         if (remoteUrl.StartsWith("http")) {
    //             return remoteUrl;
    //         }
    //
    //         if (remoteUrl.StartsWith("/")) {
    //             //site absolute URL
    //             var uri = new Uri(baseUrl);
    //             if (Uri.TryCreate(new Uri(uri.GetLeftPart(UriPartial.Authority)), remoteUrl, out var result)) {
    //                 return result.ToString();
    //             }
    //         } else {
    //             if (Uri.TryCreate(new Uri(baseUrl), remoteUrl, out var result)) {
    //                 return result.ToString();
    //             }
    //         }
    //
    //         return remoteUrl;
    //     }
    // }
}
