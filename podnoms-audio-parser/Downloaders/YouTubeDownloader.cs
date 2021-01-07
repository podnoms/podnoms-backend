using System.Threading.Tasks;

namespace PodNoms.AudioParsing.Downloaders {
    public class YouTubeDownloader : IDownloader {
        public async Task<string> DownloadFromUrl(string url, string callbackUrl) {
            return await new YtDlDownloader().DownloadFromUrl(url, callbackUrl);
        }
    }
}
