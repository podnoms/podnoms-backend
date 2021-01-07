using System.Threading.Tasks;

namespace PodNoms.AudioParsing.Downloaders {
    public interface IDownloader {
        Task<string> DownloadFromUrl(string url, string callbackUrl);
    }
}
