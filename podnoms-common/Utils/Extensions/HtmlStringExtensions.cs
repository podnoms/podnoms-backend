using System.Collections.Generic;
using System.IO;
using System.Linq;
using HtmlAgilityPack;

namespace PodNoms.Common.Utils.Extensions {
    public static class HtmlStringExtensions {
        public static string RemoveUnwantedHtmlTags(
                    this string html) {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            var sw = new StringWriter();
            _convertTo(doc.DocumentNode, sw);
            sw.Flush();
            return sw.ToString();
        }
        private static void _convertContentTo(HtmlNode node, TextWriter outText) {
            foreach (HtmlNode subnode in node.ChildNodes) {
                _convertTo(subnode, outText);
            }
        }

        private static void _convertTo(HtmlNode node, TextWriter outText) {
            string html;
            switch (node.NodeType) {
                case HtmlNodeType.Comment:
                    // don't output comments
                    break;

                case HtmlNodeType.Document:
                    _convertContentTo(node, outText);
                    break;

                case HtmlNodeType.Text:
                    // script and style must not be output
                    string parentName = node.ParentNode.Name;
                    if ((parentName == "script") || (parentName == "style"))
                        break;

                    // get text
                    html = ((HtmlTextNode)node).Text;

                    // is it in fact a special closing node output as text?
                    if (HtmlNode.IsOverlappedClosingElement(html))
                        break;

                    // check the text is meaningful and not a bunch of whitespaces
                    if (html.Trim().Length > 0) {
                        outText.Write(HtmlEntity.DeEntitize(html));
                    }
                    break;

                case HtmlNodeType.Element:
                    switch (node.Name) {
                        case "p":
                            // treat paragraphs as crlf
                            outText.Write("\r\n");
                            break;
                        case "br":
                            outText.Write("\r\n");
                            break;
                    }

                    if (node.HasChildNodes) {
                        _convertContentTo(node, outText);
                    }
                    break;
            }
        }
    }
}
