using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using PodNoms.AudioParsing.Helpers;
using YoutubeDLSharp.Metadata;

namespace PodNoms.AudioParsing.Downloaders {
    /// <summary>
    /// Process a direct mp3 link
    /// </summary>
    public class DirectDownloader : IDownloader {
        public event Action<object, string> OnOutput;
        public event Action<object, string> OnError;

        public async Task<string> DownloadFromUrl(string url, string outputFile, string callbackUrl, Dictionary<string, string> args) {
            var result = await HttpHelper.DownloadFile(url, outputFile);
            return result;
        }

        public Task<VideoData> GetVideoInformation(string url, Dictionary<string, string> args) {
            throw new System.NotImplementedException();
        }
    }
}
