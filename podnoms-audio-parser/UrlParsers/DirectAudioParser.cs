using System.Threading.Tasks;

namespace PodNoms.AudioParsing.UrlParsers {
    public class DirectAudioParser : IUrlParser {
        public async Task<bool> IsMatch(string url) {
            return await Task.FromResult(url.EndsWith(".mp3"));
        }
    }
}
