using System.Threading.Tasks;

namespace PodNoms.AudioParsing.UrlParsers {
    public class DirectAudioParser : IUrlParser {
        public async Task<bool> IsMatch(string url) {
            return await Task.FromResult(url.EndsWith(".mp3"));
        }

        public async Task<UrlType> GetType(string url) {
            return await IsMatch(url) ? UrlType.YtDl : UrlType.Invalid;
        }
    }
}
