using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using YoutubeDLSharp.Metadata;

namespace PodNoms.AudioParsing.Downloaders {
    public interface IDownloader {
        event Action<object, string> OnOutput;
        event Action<object, string> OnError;

        Task<string> DownloadFromUrl(string url, string outputFile, string callbackUrl,
            Dictionary<string, string> args = null);

        //TODO: This shouldn't be VideoData - use our own class
        Task<VideoData> GetVideoInformation(string url, Dictionary<string, string> args = null);
    }
}
