using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace PodNoms.Common.Services.PageParser {
    public class DefaultPageParser : IPageParser {

        public async Task<IList<KeyValuePair<string, string>>> GetAllAudioLinks(string url) {
            var empty = Enumerable.Empty<KeyValuePair<string, string>>();
            var document = (await GetAudioLinks(url).ConfigureAwait(false)) ?? empty;
            var iframeLinks = (await GetIFrameLinks(url).ConfigureAwait(false)) ?? empty;

            var links = document.Concat(iframeLinks)
                .Select(r => new KeyValuePair<string, string>(
                    string.IsNullOrWhiteSpace(r.Key) ? _getFilenameFromUrl(r.Value) : r.Key,
                    r.Value
                )).ToList();
            return links;
        }

        private string _getFilenameFromUrl(string value) {
            var uri = new Uri(value);
            return uri.Segments[uri.Segments.Length - 1];
        }

        public async Task<IList<KeyValuePair<string, string>>> GetIFrameLinks(string url) {
            HtmlWeb web = new HtmlWeb();
            var doc = await web.LoadFromWebAsync(url)
                .ConfigureAwait(false);

            var iframes = doc.DocumentNode.Descendants("iframe")
                .Where(r => !string.IsNullOrEmpty(r.Attributes["src"].Value.ToString()))
                .Select(r => r.Attributes["src"].Value.ToString());

            if (iframes.Count() != 0) {
                var response = await Task.WhenAll(
                        iframes.Select(async e => await GetAudioLinks(e).ConfigureAwait(false))
                        ).ConfigureAwait(false);
                return response.SelectMany(r => r).ToList();
            }
            return null;
        }
        public async Task<IList<KeyValuePair<string, string>>> GetAudioLinks(string url) {
            var empty = Enumerable.Empty<KeyValuePair<string, string>>();
            HtmlWeb web = new HtmlWeb();

            var doc = await web.LoadFromWebAsync(url)
                .ConfigureAwait(false);


            var hrefSources = doc.DocumentNode.Descendants("a")
                .Where(a => (
                            !string.IsNullOrEmpty(a.Attributes["href"]?.Value) && (
                                a.Attributes["href"].Value.EndsWith("mp3") ||
                                a.Attributes["href"].Value.EndsWith("ogg") ||
                                a.Attributes["href"].Value.EndsWith("wav") ||
                                a.Attributes["href"].Value.EndsWith("m4a")
                                )
                            ))
                .Select(d => new KeyValuePair<string, string>(
                    Regex.Replace(d.InnerText, @"\s+", ""),
                    _normaliseUrl(url, d.Attributes["href"].Value)
                )) ?? empty;

            var audioSources = doc.DocumentNode.Descendants("audio")
                .Where(n => n.Attributes["src"] != null)
                .Select(d => new KeyValuePair<string, string>(
                    Path.GetFileName(d.Attributes["src"].Value),
                    _normaliseUrl(url, d.Attributes["src"].Value)
                )) ?? empty;

            var embeddedAudioSources = doc.DocumentNode.Descendants("audio")
                .Where(n => n.HasChildNodes)
                .SelectMany(r => r.ChildNodes.Where(n =>
                            n.Attributes["src"] != null &&
                            n.Attributes["type"] != null && (
                                n.Attributes["type"].Value == "audio/mp3" ||
                                n.Attributes["type"].Value == "audio/ogg" ||
                                n.Attributes["type"].Value == "audio/wav" ||
                                n.Attributes["type"].Value == "audio/m4a"
                                )
                            ))
                .Select(d => new KeyValuePair<string, string>(
                    Path.GetFileName(d.Attributes["src"].Value),
                    _normaliseUrl(url, d.Attributes["src"].Value)
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
