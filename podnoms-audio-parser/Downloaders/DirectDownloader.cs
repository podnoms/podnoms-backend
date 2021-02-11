using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PodNoms.AudioParsing.Helpers;
using PodNoms.AudioParsing.Models;
using YoutubeDLSharp.Metadata;

namespace PodNoms.AudioParsing.Downloaders {
    /// <summary>
    /// Process a direct mp3 link
    /// </summary>
    public class DirectDownloader : IDownloader {
        public event Action<object, string> OnOutput;
        public event Action<object, string> OnError;

        public async Task<string> DownloadFromUrl(string url, string outputFile,
            Dictionary<string, string> args = null, Func<ProcessingProgress, Task<bool>> progressCallback = null) {
            var result = await HttpHelper.DownloadFile(url, outputFile);
            return result;
        }

        public async Task<VideoData> GetVideoInformation(string url, Dictionary<string, string> args = null) {
            var result = new VideoData {
                Title = new Uri(url).Segments[^1],
                Description = string.Empty,
                Thumbnail = "https://cdn.podnoms.com/static/images/default-entry.png"
            };
            return await Task.FromResult(result);
        }
    }
}
