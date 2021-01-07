using System;
using System.IO;
using System.Threading.Tasks;
using PodNoms.AudioParsing.Helpers;
using YoutubeDLSharp;
using YoutubeDLSharp.Options;

namespace PodNoms.AudioParsing.Downloaders {
    public class YtDlDownloader : IDownloader {
        public async Task<string> DownloadFromUrl(string url, string callbackUrl) {
            var id = System.Guid.NewGuid().ToString();

            var templateFile = Path.Combine(Path.GetTempPath(), $"{id}.%(ext)s");

            var ytdl = new YoutubeDL {
                YoutubeDLPath = "/usr/bin/youtube-dl",
                FFmpegPath = "/usr/bin/ffmpeg",
                OutputFolder = Path.GetTempPath()
            };

            var result = await ytdl.RunAudioDownload(
                url,
                AudioConversionFormat.Mp3,
                overrideOptions: new OptionSet() {
                    Output = templateFile
                });

            var resultFile = result.Data.Replace("'", "");
            return File.Exists(resultFile)
                ? resultFile
                : string.Empty;
        }
    }
}
