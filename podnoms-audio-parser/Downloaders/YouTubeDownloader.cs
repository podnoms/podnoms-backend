using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PodNoms.AudioParsing.Models;
using YoutubeDLSharp.Metadata;

namespace PodNoms.AudioParsing.Downloaders {
    public class YouTubeDownloader : IDownloader {
        public event Action<object, string> OnOutput;
        public event Action<object, string> OnError;

        public async Task<string> DownloadFromUrl(string url, string outputFile,
            Dictionary<string, string> args = null, Func<ProcessingProgress, Task<bool>> progressCallback = null) {
            return await new YtDlDownloader().DownloadFromUrl(url, outputFile, args, progressCallback);
        }

        public async Task<VideoData> GetVideoInformation(string url, Dictionary<string, string> args) {
            return await new YtDlDownloader().GetVideoInformation(url, args);
        }
    }
}
