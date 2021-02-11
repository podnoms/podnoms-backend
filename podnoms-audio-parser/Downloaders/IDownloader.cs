using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PodNoms.AudioParsing.Models;
using YoutubeDLSharp.Metadata;

namespace PodNoms.AudioParsing.Downloaders {
    public interface IDownloader {
        Task<string> DownloadFromUrl(string url, string outputFile,
            Dictionary<string, string> args = null, Func<ProcessingProgress, Task<bool>> progressCallback = null);

        //TODO: This shouldn't be VideoData - use our own class
        Task<VideoData> GetVideoInformation(string url, Dictionary<string, string> args = null);
    }
}
