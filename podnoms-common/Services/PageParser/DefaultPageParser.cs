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
            var document = (await GetAudioLinks(url)
                    .ConfigureAwait(false))
                .SelectMany(r => r, (k, v) => new { Key = k.Key, Value = v });
            var iframeLinks = (await GetIFrameLinks(url)
                    .ConfigureAwait(false))
                .SelectMany(r => r, (k, v) => new { Key = k.FirstOrDefault().Key, Value = v.FirstOrDefault() })
                .ToList();
            var links = document.Concat(iframeLinks)
                .Select(r => new KeyValuePair<string, string>(r.Key, r.Value))
                .ToList();

            return links;
        }
        public async Task<IList<ILookup<string, string>>> GetIFrameLinks(string url) {
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
                return response;
            }
            return null;
        }
        public async Task<ILookup<string, string>> GetAudioLinks(string url) {
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
                .Select(d => new {
                        Name = Regex.Replace(d.InnerText, @"\s+", ""),
                        SourceUrl = _normaliseUrl(url, d.Attributes["href"].Value)
                        });

            var audioSources = doc.DocumentNode.Descendants("audio")
                .Where(n => n.Attributes["src"] != null)
                .Select(d => new {
                        Name = Path.GetFileName(d.Attributes["src"].Value),
                        SourceUrl = _normaliseUrl(url, d.Attributes["src"].Value)
                        });
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
                .Select(d => new {
                        Name = Path.GetFileName(d.Attributes["src"].Value),
                        SourceUrl = _normaliseUrl(url, d.Attributes["src"].Value)
                        });
            return hrefSources
                .Concat(audioSources)
                .Concat(embeddedAudioSources)
                .ToLookup(r => r.Name, r => r.SourceUrl);
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
