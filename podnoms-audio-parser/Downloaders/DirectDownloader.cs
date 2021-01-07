using System.IO;
using System.Threading.Tasks;
using PodNoms.AudioParsing.Helpers;

namespace PodNoms.AudioParsing.Downloaders {
    /// <summary>
    /// Process a direct mp3 link
    /// </summary>
    public class DirectDownloader : IDownloader {
        public async Task<string> DownloadFromUrl(string url, string callbackUrl) {
            var outputFile = PathHelper.GetTempFileNameWithExtension(".mp3");
            var result = await HttpHelper.DownloadFile(url, outputFile);
            return result;
        }
    }
}
