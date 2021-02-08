using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using YoutubeDLSharp.Metadata;

namespace PodNoms.AudioParsing.Downloaders {
    public class YouTubeDownloader : IDownloader {
        public event Action<object, string> OnOutput;
        public event Action<object, string> OnError;

        public async Task<string> DownloadFromUrl(string url, string outputFile, string callbackUrl,
            Dictionary<string, string> args) {
            return await new YtDlDownloader().DownloadFromUrl(url, outputFile, callbackUrl, args);
        }

        public async Task<VideoData> GetVideoInformation(string url, Dictionary<string, string> args) {
            return await new YtDlDownloader().GetVideoInformation(url, args);
        }
    }
}
