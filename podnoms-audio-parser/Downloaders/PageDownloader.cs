using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PodNoms.AudioParsing.Models;
using YoutubeDLSharp.Metadata;

namespace PodNoms.AudioParsing.Downloaders {
    public class PageDownloader : IDownloader {
        public event Action<object, string> OnOutput;
        public event Action<object, string> OnError;

        public Task<string> DownloadFromUrl(string url, string outputFile, Dictionary<string, string> args = null,
            Func<ProcessingProgress, Task<bool>> progressCallback = null) {
            throw new System.NotImplementedException();
        }

        public Task<VideoData> GetVideoInformation(string url, Dictionary<string, string> args) {
            throw new System.NotImplementedException();
        }
    }
}
