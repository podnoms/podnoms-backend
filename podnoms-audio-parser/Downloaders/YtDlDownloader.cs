using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using PodNoms.AudioParsing.ErrorHandling;
using PodNoms.AudioParsing.Helpers;
using PodNoms.AudioParsing.Models;
using YoutubeDLSharp;
using YoutubeDLSharp.Metadata;
using YoutubeDLSharp.Options;

namespace PodNoms.AudioParsing.Downloaders {
    public class YtDlDownloader : IDownloader {
        private const string DOWNLOADRATESTRING = "iB/s";
        private const string DOWNLOADSIZESTRING = "iB";
        protected const string OFSTRING = "of";

        private static ProcessingProgress _parseProgress(string output) {
            var result = new ProcessingProgress(
                new TransferProgress()) {
                ProcessingStatus = "Downloading"
            };
            try {
                var progressIndex = output.LastIndexOf(' ', output.IndexOf('%')) + 1;
                var progressString = output.Substring(progressIndex, output.IndexOf('%') - progressIndex);
                ((TransferProgress)result.Payload).Percentage = (int)Math.Round(double.Parse(progressString));

                var sizeIndex = output.LastIndexOf(' ', output.IndexOf(DOWNLOADSIZESTRING, StringComparison.Ordinal)) +
                                1;
                var sizeString = output.Substring(sizeIndex,
                    output.IndexOf(DOWNLOADSIZESTRING, StringComparison.Ordinal) - sizeIndex + 2);
                ((TransferProgress)result.Payload).TotalSize = sizeString;

                if (!output.Contains(DOWNLOADRATESTRING)) {
                    return result;
                }

                var rateIndex =
                    output.LastIndexOf(' ', output.LastIndexOf(DOWNLOADRATESTRING, StringComparison.Ordinal)) + 1;
                var rateString = output.Substring(rateIndex,
                    output.LastIndexOf(DOWNLOADRATESTRING, StringComparison.Ordinal) - rateIndex + 4);
                result.Progress = rateString;
            } catch (Exception) {
                // ignored
            }

            return result;
        }

        public async Task<string> DownloadFromUrl(string url, string outputFile, Dictionary<string, string> args = null,
            Func<ProcessingProgress, Task<bool>> progressCallback = null) {
            var ytdl = new YoutubeDLProcess(args != null && args.ContainsKey("Downloader")
                ? args["Downloader"]
                : "youtube-dl");

            var options = new OptionSet() {
                Output = outputFile.ReplaceEnd("mp3", "%(ext)s"),
                ExtractAudio = true,
                AudioFormat = AudioConversionFormat.Mp3,
                AudioQuality = 0,
            };

            if (progressCallback != null) {
                ytdl.OutputReceived += (sender, eventArgs) => {
                    var progress = _parseProgress(eventArgs.Data);
                    progressCallback(progress);
                };
            }

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
