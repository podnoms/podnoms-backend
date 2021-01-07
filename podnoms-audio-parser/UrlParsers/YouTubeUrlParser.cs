using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PodNoms.AudioParsing.UrlParsers {
    public class YouTubeUrlParser : IUrlParser {
        const string UrlRegex = @"^(http(s)?:\/\/)?((w){3}.)?youtu(be|.be)?(\.com)?\/.+";

        public async Task<bool> IsMatch(string url) {
            var regex = new Regex(UrlRegex);
            var result = regex.Match(url);
            return await Task.FromResult(result.Success);
        }
    }
}
