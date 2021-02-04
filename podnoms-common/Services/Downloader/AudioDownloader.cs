using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nito.AsyncEx.Synchronous;
using PodNoms.AudioParsing.Downloaders;
using PodNoms.AudioParsing.ErrorHandling;
using PodNoms.Common.Data.Settings;
using PodNoms.Common.Data.ViewModels;
using PodNoms.Common.Utils;
using PodNoms.Common.Utils.RemoteParsers;
using PodNoms.Data.Enums;
using YoutubeDLSharp.Metadata;


namespace PodNoms.Common.Services.Downloader {
    public class AudioDownloader {
        private readonly IYouTubeParser _youTubeParser;
        private readonly IDownloader _downloader;
        private readonly ILogger<AudioDownloader> _logger;

        private static readonly List<string> _audioFileTypes = new List<string>() {
            "audio/mpeg"
        };

        public RemoteVideoInfo Properties { get; set; }
        public VideoData RawProperties { get; private set; }

        private const string DOWNLOADRATESTRING = "iB/s";
        private const string DOWNLOADSIZESTRING = "iB";
        protected const string OFSTRING = "of";
        private readonly HelpersSettings _helpersSettings;

        public event EventHandler<ProcessingProgress> DownloadProgress;

        public AudioDownloader(IYouTubeParser youTubeParser, IOptions<HelpersSettings> helpersSettings,
            IDownloader downloader, ILogger<AudioDownloader> logger) {
            _helpersSettings = helpersSettings.Value;
            _youTubeParser = youTubeParser;
            _downloader = downloader;
            _logger = logger;
        }

        private static async Task<bool> _remoteIsAudio(string url) =>
            url.Contains("drive.google.com") ||
            url.Contains("dl.dropboxusercontent.com") ||
            url.EndsWith(".mp3") ||
            _audioFileTypes.Contains(await HttpUtils.GetRemoteMimeType(url));

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

        void _handleError(string error) {
            if (error.Contains("ERROR: Unsupported URL") ||
                error.Contains("ERROR: There's no video in this")) {
                throw new AudioDownloadException(error);
            }
        }

        private async Task<VideoData> __getInfo(string url) {
            try {
                var result = await _downloader.GetVideoInformation(url);
                throw new AudioDownloadException("No audio found in that URL");
            } catch (TaskCanceledException) {
                _logger.LogError("Unable to parse url");
            } catch (Exception e) {
                _logger.LogError($"Error geting info for {url}");
                _logger.LogError(e.Message);
            }

            return null;
        }

        public async Task<RemoteUrlType> GetInfo(string url, string userId) {
            var ret = RemoteUrlType.Invalid;
            if (await _remoteIsAudio(url)) {
                return RemoteUrlType.SingleItem;
            }

            if (_youTubeParser.ValidateUrl(url)) {
                //we're youtube. bypass youtube_dl for info - it's very slow
                var urlType = await _youTubeParser.GetUrlType(url);
                if (urlType == RemoteUrlType.SingleItem) {
                    Properties = await _youTubeParser.GetVideoInformation(url, userId);
                }

                if (!string.IsNullOrEmpty(Properties?.Title)) {
                    return urlType;
                }
            }

            try {
                var info = await __getInfo(url);
                if (info == null) {
                    return ret;
                }

                var props = new RemoteVideoInfo {
                    Title = info.Title,
                    Description = info.Description,
                    Thumbnail = info.Thumbnails.FirstOrDefault(r => !string.IsNullOrEmpty(r?.Url))?.Url,
                    Uploader = info.Uploader,
                    UploadDate = (info?.UploadDate ?? System.DateTime.Now).ToString().ParseBest(),
                    VideoId = info.ID
                };

                Properties = props;

                // ret = info switch {
                //     // have to dump playlist handling for now
                //     PlaylistDownloadInfo downloadInfo when downloadInfo.Videos.Count > 0 => RemoteUrlType.Playlist,
                //     VideoDownloadInfo _ => RemoteUrlType.SingleItem,
                //     _ => ret
                // };

                //TODO: ^^ handle playlists above
                return RemoteUrlType.SingleItem;
            } catch (AudioDownloadException) {
                return RemoteUrlType.Invalid;
            }
        }

        public async Task<string> DownloadAudio(string id, string url, string outputFile = "") {
            if (string.IsNullOrEmpty(outputFile)) {
                outputFile = Path.Combine(Path.GetTempPath(), $"{id}.mp3");
            }

            if (await _remoteIsAudio(url)) {
                return _downloadFileDirect(url, outputFile);
            }

            _downloader.OnError += (sender, output) => {
                _logger.LogError($"YTERR: {output}");
            };
            _downloader.OnOutput += (sender, output) => {
                _logger.LogError($"YTERR: {output}");
                if (output.Contains("%")) {
                    try {
                        var progress = _parseProgress(output);
                        DownloadProgress?.Invoke(this, progress);
                    } catch (Exception ex) {
                        _logger.LogError($"Error parsing progress {ex.Message}");
                    }
                } else {
                    DownloadProgress?.Invoke(this, new ProcessingProgress(null) {
                        ProcessingStatus = ProcessingStatus.Converting,
                        Progress = _statusLineToNarrative(output)
                    });
                }
            };
            outputFile = await _downloader.DownloadFromUrl(url, outputFile, string.Empty, new Dictionary<string, string> {
                {"Downloader", _helpersSettings.Downloader},
                {"FFMPeg", _helpersSettings.Downloader}
            });
            return File.Exists(outputFile) ? outputFile : string.Empty;
        }

        private static string _statusLineToNarrative(string output) =>
            output.Contains(":") ? output.Split(':')[1] : "Converting (this may take a bit)";

        private static ProcessingProgress _parseProgress(string output) {
            var result = new ProcessingProgress(
                new TransferProgress()) {
                ProcessingStatus = ProcessingStatus.Downloading
            };

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
