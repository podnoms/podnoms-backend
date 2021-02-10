using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using PodNoms.AudioParsing.ErrorHandling;
using PodNoms.AudioParsing.Helpers;
using YoutubeDLSharp;
using YoutubeDLSharp.Metadata;
using YoutubeDLSharp.Options;

namespace PodNoms.AudioParsing.Downloaders {
    public class YtDlDownloader : IDownloader {
        public event Action<object, string> OnOutput;
        public event Action<object, string> OnError;

        public async Task<string> DownloadFromUrl(string url, string outputFile,
            string callbackUrl, Dictionary<string, string> args) {
            var ytdl = new YoutubeDLProcess(args != null && args.ContainsKey("Downloader")
                ? args["Downloader"]
                : "youtube-dl");

            var options = new OptionSet() {
                Output = outputFile,
                ExtractAudio = true,
                AudioFormat = AudioConversionFormat.Mp3,
                AudioQuality = 0,
            };
            var result = await ytdl.RunAsync(new string[] {url}, options);

            return result == 0 && File.Exists(outputFile) ? outputFile : string.Empty;
        }

        public async Task<VideoData> GetVideoInformation(string url, Dictionary<string, string> args = null) {
            var ytdl = new YoutubeDL() {
                YoutubeDLPath = args != null && args.ContainsKey("Downloader") ? args["Downloader"] : "youtube-dl",
                FFmpegPath = args != null && args.ContainsKey("FFMPeg") ? args["FFMPeg"] : "/usr/bin/ffmpeg",
                OutputFolder = Path.GetTempPath(),
            };

            RunResult<VideoData> result = await ytdl.RunVideoDataFetch(url);
            if (result.Success) {
                return result.Data;
            }

            throw new AudioDownloadException($"Unable to get url information\n${url}");
        }
    }
}
