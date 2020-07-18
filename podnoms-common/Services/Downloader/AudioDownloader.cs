using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Nito.AsyncEx.Synchronous;
using PodNoms.Common.Data.Settings;
using PodNoms.Common.Data.ViewModels;
using NYoutubeDL;
using NYoutubeDL.Models;
using NYoutubeDL.Helpers;
using PodNoms.Common.Utils;
using PodNoms.Common.Utils.RemoteParsers;
using PodNoms.Data.Enums;

namespace PodNoms.Common.Services.Downloader {
    public class AudioDownloader {
        private readonly IYouTubeParser _youTubeParser;

        private VideoDownloadInfo __Properties => RawProperties is VideoDownloadInfo info ? info : null;
        public RemoteVideoInfo Properties { get; set; }
        public DownloadInfo RawProperties { get; private set; }

        private const string DOWNLOADRATESTRING = "iB/s";
        private const string DOWNLOADSIZESTRING = "iB";
        protected const string OFSTRING = "of";
        private readonly HelpersSettings _helpersSettings;

        public event EventHandler<ProcessingProgress> DownloadProgress;

        public AudioDownloader(IYouTubeParser youTubeParser, IOptions<HelpersSettings> helpersSettings) {
            _helpersSettings = helpersSettings.Value;
            _youTubeParser = youTubeParser;
        }

        public static string GetVersion(string downloader) {
            try {
                var proc = new Process {
                    StartInfo = new ProcessStartInfo {
                        FileName = downloader,
                        Arguments = $"--version",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };
                var br = new StringBuilder();
                proc.Start();
                while (!proc.StandardOutput.EndOfStream) {
                    br.Append(proc.StandardOutput.ReadLine());
                }

                return br.ToString();
            } catch (Exception ex) {
                return $"{{\"Error\": \"{ex?.Message}\"}}";
            }
        }

        private async Task<DownloadInfo> __getInfo(string url) {
            try {
                var yt = new YoutubeDL { VideoUrl = url };

                yt.StandardErrorEvent += (sender, e) => {
                    if (e.Contains("ERROR: Unsupported URL")) {
                        yt.CancelDownload();
                    }
                };

                var cmdLine = await yt.PrepareDownloadAsync();
                Console.WriteLine($"Getting download info {cmdLine}");
                var info = await yt.GetDownloadInfoAsync();
                if (info is null ||
                    info.Errors.Count != 0 ||
                    (info.GetType() == typeof(PlaylistDownloadInfo) &&
                     !MixcloudParser.ValidateUrl(url) &&
                     !_youTubeParser.ValidateUrl(url))) {
                    return null;
                }
                return info;
            } catch (TaskCanceledException) {
                Console.WriteLine("Unable to parse url");
            } catch (Exception e) {
                Console.WriteLine($"Error geting info for {url}");
                Console.WriteLine(e.Message);
            }

            return null;
        }

        public async Task<string> GetChannelId(string url) {
            var info = await __getInfo(url);
            return info?.Title;
        }

        public async Task<RemoteUrlType> GetInfo(string url, bool goDeep = false) {
            var ret = RemoteUrlType.Invalid;

            if (url.Contains("drive.google.com") ||
                    url.Contains("dl.dropboxusercontent.com") ||
                    url.EndsWith(".mp3")) {
                return RemoteUrlType.SingleItem;
            }

            if (_youTubeParser.ValidateUrl(url)) {
                //we're youtube. bypass youtube_dl for info - it's very slow
                var urlType = await _youTubeParser.GetUrlType(url);
                if (urlType == RemoteUrlType.SingleItem) {
                    Properties = await _youTubeParser.GetInformation(url);
                }
                return urlType;
            }

            var info = await __getInfo(url);
            if (info == null) {
                return ret;
            }

            RawProperties = info;
            var parsed = info as VideoDownloadInfo;

            Properties = new RemoteVideoInfo {
                Title = parsed?.Title,
                Description = parsed?.Description,
                Thumbnail = parsed?.Thumbnails.FirstOrDefault(r => !string.IsNullOrEmpty(r?.Url))?.Url,
                Uploader = parsed?.Description,
                UploadDate = (parsed?.UploadDate ?? System.DateTime.Now.ToString()).ParseBest(),
                VideoId = parsed?.Id
            };

            switch (info) {
                // have to dump playlist handling for now
                case PlaylistDownloadInfo _ when ((PlaylistDownloadInfo)info).Videos.Count > 0:
                    ret = RemoteUrlType.Playlist;
                    break;
                case VideoDownloadInfo _:
                    ret = RemoteUrlType.SingleItem;
                    break;
            }

            return ret;
        }

        public async Task<string> DownloadAudio(string id, string url, string outputFile = "") {

            if (string.IsNullOrEmpty(outputFile)) {

            }
            var templateFile = string.IsNullOrEmpty(outputFile) ?
                    Path.Combine(Path.GetTempPath(), $"{id}.%(ext)s") :
                outputFile.Replace(".mp3", ".%(ext)s"); //hacky but can't see a way to specify final filename

            if (url.Contains("drive.google.com")) {
                return _downloadFileDirect(url, outputFile);
            }

            var cleanedUrl = await _normaliseUrl(url);

            var yt = new YoutubeDL();
            yt.Options.FilesystemOptions.Output = templateFile;
            yt.Options.PostProcessingOptions.ExtractAudio = true;
            yt.Options.PostProcessingOptions.AudioFormat = Enums.AudioFormat.mp3;
            yt.Options.PostProcessingOptions.AudioQuality = "0";

            yt.VideoUrl = cleanedUrl;

            yt.StandardOutputEvent += (sender, output) => {
                if (output.StartsWith("ERROR:")) {
                    throw new AudioDownloadException(output);
                }

                if (output.Contains("%")) {
                    try {
                        var progress = _parseProgress(output);
                        DownloadProgress?.Invoke(this, progress);
                    } catch (Exception ex) {
                        Console.WriteLine($"Error parsing progress {ex.Message}");
                    }
                } else {
                    DownloadProgress?.Invoke(this, new ProcessingProgress(null) {
                        ProcessingStatus = ProcessingStatus.Converting,
                        Progress = _statusLineToNarrative(output)
                    });
                    ;
                    Console.WriteLine(output);
                }
            };
            var commandText = yt.PrepareDownload();
            Console.WriteLine(commandText);
            Console.WriteLine(yt.RunCommand);

            await yt.DownloadAsync();
            return File.Exists(outputFile) ? outputFile : string.Empty;
        }

        private async Task<string> _normaliseUrl(string url) {
            if (_youTubeParser.ValidateUrl(url)) {
                return $"https://www.youtube.com/watch?v={await _youTubeParser.GetVideoId(url)}";
            }

            return url;
        }

        private string _statusLineToNarrative(string output) {
            //[youtube] rzfmZC3kg3M: Downloading webpage
            if (output.Contains(":")) {
                return output.Split(':')[1];
            }

            return "Converting (this may take a bit)";
        }

        private ProcessingProgress _parseProgress(string output) {
            var result = new ProcessingProgress(new TransferProgress());
            result.ProcessingStatus = ProcessingStatus.Downloading;

            var progressIndex = output.LastIndexOf(' ', output.IndexOf('%')) + 1;
            var progressString = output.Substring(progressIndex, output.IndexOf('%') - progressIndex);
            ((TransferProgress)result.Payload).Percentage = (int)Math.Round(double.Parse(progressString));

            var sizeIndex = output.LastIndexOf(' ', output.IndexOf(DOWNLOADSIZESTRING, StringComparison.Ordinal)) + 1;
            var sizeString = output.Substring(sizeIndex,
                output.IndexOf(DOWNLOADSIZESTRING, StringComparison.Ordinal) - sizeIndex + 2);
            ((TransferProgress)result.Payload).TotalSize = sizeString;

            if (output.Contains(DOWNLOADRATESTRING)) {
                var rateIndex =
                    output.LastIndexOf(' ', output.LastIndexOf(DOWNLOADRATESTRING, StringComparison.Ordinal)) + 1;
                var rateString = output.Substring(rateIndex,
                    output.LastIndexOf(DOWNLOADRATESTRING, StringComparison.Ordinal) - rateIndex + 4);
                result.Progress = rateString;
            }

            return result;
        }

        private string _downloadFileDirect(string url, string fileName) {
            var file = HttpUtils.DownloadFile(url, fileName).WaitAndUnwrapException();
            return file;
        }
    }
}
