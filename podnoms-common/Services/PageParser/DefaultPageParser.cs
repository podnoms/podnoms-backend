using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace PodNoms.Common.Services.PageParser {
    public class DefaultPageParser : IPageParser {
        private const string PARSER_REGEX =
            @"((https?|ftp|gopher|telnet|file|notes|ms-help):((//)|(\\\\))+[\w\d:#@%/;$()~_?\+-=\\\.&]*\.mp3)";

        private HtmlDocument _doc;
        private string _url;

        private static async Task<DefaultPageParser> __create(string url) {
            var cls = new DefaultPageParser();
            await cls.Initialise(url);
            return cls;
        }

        public bool _validateUrl(string url) {
            if (Uri.TryCreate(url, UriKind.Absolute, out var validatedUri)) {
                return (validatedUri.Scheme == Uri.UriSchemeHttp || validatedUri.Scheme == Uri.UriSchemeHttps);
            }

            return false;
        }

        public async Task<bool> Initialise(string url) {
            if (_validateUrl(url)) {
                this._url = url;
                HtmlWeb web = new HtmlWeb();
                _doc = await web.LoadFromWebAsync(url);
                return true;
            }

            return false;
        }

        public string GetHeadTag(string tagName) {
            return _doc.DocumentNode
                .SelectSingleNode($"//meta[@property='{tagName}']")?
                .Attributes["content"].Value ?? string.Empty;
        }

        public string GetPageTitle() {
            if (_doc == null) {
                throw new InvalidOperationException("Initialise must be called first");
            }

            return _doc.DocumentNode.SelectSingleNode("//title").InnerText;
        }

        public string GetPageText() {
            if (_doc == null) {
                throw new InvalidOperationException("Initialise must be called first");
            }

            return _doc.Text;
        }

        public async Task<IList<KeyValuePair<string, string>>> GetAllAudioLinks() {
            var empty = Enumerable.Empty<KeyValuePair<string, string>>();
            var documentLinks = (GetAudioLinks()) ?? empty;
            var iframeLinks = (await GetIFrameLinks())?.Select(r => new KeyValuePair<string, string>(
                string.IsNullOrWhiteSpace(r.Key) ? _getFilenameFromUrl(r.Value) : r.Key,
                r.Value
            )).ToList() ?? empty;
            var textLinks = GetTextLinks(GetPageText());

            var links = documentLinks.Concat(iframeLinks).Concat(textLinks);
            return links
                .Distinct()
                .ToList();
        }

        public IList<KeyValuePair<string, string>> GetTextLinks(string text) {
            var matches = Regex.Matches(text, PARSER_REGEX)
                .Cast<Match>()
                .Select(m => m.Value)
                .ToArray()
                .Select(r => new KeyValuePair<string, string>(
                    _getFilenameFromUrl(r),
                    r
                )).ToList();
            return matches;
        }

        private string _getFilenameFromUrl(string value) {
            var uri = new Uri(value);
            return uri.Segments[uri.Segments.Length - 1];
        }

        public async Task<IList<KeyValuePair<string, string>>> GetIFrameLinks() {
            if (_doc == null) {
                throw new InvalidOperationException("Initialise must be called first");
            }

            var iframes = _doc.DocumentNode.Descendants("iframe")
                .Where(r => !string.IsNullOrEmpty(r.Attributes["src"]?.Value.ToString()))
                .Where(r => r.Attributes["src"].Value.ToString().StartsWith("http"))
                .Select(r => r.Attributes["src"].Value.ToString());

            if (iframes?.Any() == true) {
                var response = await Task.WhenAll(
                    iframes
                        .Select(async e => (await DefaultPageParser.__create(e)).GetAudioLinks())
                ).ConfigureAwait(false);
                return response.SelectMany(r => r).ToList();
            }

            return null;
        }

        public IList<KeyValuePair<string, string>> GetAudioLinks() {
            if (_doc == null) {
                throw new InvalidOperationException("Initialise must be called first");
            }

            var empty = Enumerable.Empty<KeyValuePair<string, string>>();
            HtmlWeb web = new HtmlWeb();

            var hrefSources = _doc.DocumentNode.Descendants("a")
                .Where(a => (!string.IsNullOrEmpty(a.Attributes["href"]?.Value) && (
                    a.Attributes["href"].Value.EndsWith("mp3") ||
                    a.Attributes["href"].Value.EndsWith("ogg") ||
                    a.Attributes["href"].Value.EndsWith("wav") ||
                    a.Attributes["href"].Value.EndsWith("m4a")
                )))
                .Select(d => new KeyValuePair<string, string>(
                    Regex.Replace(d.InnerText, @"\s+", ""),
                    _normaliseUrl(_url, d.Attributes["href"].Value)
                )) ?? empty;

            var audioSources = _doc.DocumentNode.Descendants("audio")
                .Where(n => n.Attributes["src"] != null)
                .Select(d => new KeyValuePair<string, string>(
                    Path.GetFileName(d.Attributes["src"].Value),
                    _normaliseUrl(_url, d.Attributes["src"].Value)
                )) ?? empty;

            var embeddedAudioSources = _doc.DocumentNode.Descendants("audio")
                .Where(n => n.HasChildNodes)
                .SelectMany(r => r.ChildNodes.Where(n =>
                    n.Attributes["src"] != null &&
                    n.Attributes["type"] != null && (
                        n.Attributes["type"].Value == "audio/mp3" ||
                        n.Attributes["type"].Value == "audio/mpeg" ||
                        n.Attributes["type"].Value == "audio/ogg" ||
                        n.Attributes["type"].Value == "audio/wav" ||
                        n.Attributes["type"].Value == "audio/m4a"
                    )
                ))
                .Select(d => new KeyValuePair<string, string>(
                    Path.GetFileName(d.Attributes["src"].Value),
                    _normaliseUrl(_url, d.Attributes["src"].Value)
                )) ?? empty;

            return hrefSources
                .Concat(audioSources)
                .Concat(embeddedAudioSources)
                .ToList();
        }

        private string _normaliseUrl(string baseUrl, string remoteUrl) {
            if (!remoteUrl.StartsWith("http")) {
                if (remoteUrl.StartsWith("/")) {
                    //site absolute URL
                    var uri = new Uri(baseUrl);
                    if (Uri.TryCreate(new Uri(uri.GetLeftPart(UriPartial.Authority)), remoteUrl, out var result)) {
                        return result.ToString();
                    }
                } else {
                    if (Uri.TryCreate(new Uri(baseUrl), remoteUrl, out var result)) {
                        return result.ToString();
                    }
                }
            }

            return remoteUrl;
        }
    }
}
