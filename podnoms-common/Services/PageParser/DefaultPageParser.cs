using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace PodNoms.Common.Services.PageParser {
    public class DefaultPageParser : IPageParser {
        public async Task<Dictionary<string, string>> GetAudioLinks(string url) {
            HtmlWeb web = new HtmlWeb();

            var doc = await web.LoadFromWebAsync(url);
            var nodes = doc.DocumentNode.SelectNodes("//a[@href]");

            var links = doc.DocumentNode
                .Descendants("audio")
                .SelectMany(a =>
                    a.ChildNodes.Where(n => n.GetType() != typeof(HtmlTextNode))
                )
                .ToList()
                .Select(n => new KeyValuePair<string, HtmlNode>(n.Id, n));

            return new Dictionary<string, string>();
        }
    }
}