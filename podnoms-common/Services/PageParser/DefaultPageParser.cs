using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace PodNoms.Common.Services.PageParser {
    public class DefaultPageParser : IPageParser {
        public async Task<Dictionary<string, string>> GetAudioLinks(string url) {
            HtmlWeb web = new HtmlWeb();

            var doc = await web.LoadFromWebAsync(url);
            var audioSources = doc.DocumentNode.Descendants("a")
                .Where(a => a.Attributes["href"].Value.EndsWith("mp3"))
            .Select(d => new {
                Name = Regex.Replace(d.InnerText, @"\s+", ""),
                SourceUrl = d.Attributes["href"].Value
            }).Concat(
                doc.DocumentNode.Descendants("audio")
                    .Where(n => n.Attributes["src"] != null)
                    .Select(d => new {
                        Name = Path.GetFileName(d.Attributes["src"].Value),
                        SourceUrl = d.Attributes["src"].Value
                    })
                    .Concat(doc.DocumentNode.Descendants("audio")
                        .Where(n => n.HasChildNodes)
                        .SelectMany(r => r.ChildNodes.Where(
                            n => n.Attributes["src"] != null &&
                            n.Attributes["type"] != null &&
                            n.Attributes["type"].Value == "audio/mp3")
                        )
                        .Select(d => new {
                            Name = Path.GetFileName(d.Attributes["src"].Value),
                            SourceUrl = d.Attributes["src"].Value
                        })
            ));
            return audioSources.ToDictionary(r => r.Name, r => r.SourceUrl);
        }
    }
}