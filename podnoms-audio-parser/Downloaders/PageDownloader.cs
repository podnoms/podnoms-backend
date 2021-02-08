using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using YoutubeDLSharp.Metadata;

namespace PodNoms.AudioParsing.Downloaders {
    public class PageDownloader : IDownloader {
        public event Action<object, string> OnOutput;
        public event Action<object, string> OnError;

        public Task<string> DownloadFromUrl(string url, string outputFile, string callbackUrl, Dictionary<string, string> args) {
            throw new System.NotImplementedException();
        }

        public Task<VideoData> GetVideoInformation(string url, Dictionary<string, string> args) {
            throw new System.NotImplementedException();
        }
    }
}
