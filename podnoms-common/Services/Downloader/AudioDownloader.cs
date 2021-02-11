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
using PodNoms.AudioParsing.Models;
using PodNoms.Common.Data.Settings;
using PodNoms.Common.Data.ViewModels;
using PodNoms.Common.Services.Realtime;
using PodNoms.Common.Utils;
using PodNoms.Common.Utils.RemoteParsers;
using YoutubeDLSharp.Metadata;
using ProcessingStatus = PodNoms.Data.Enums.ProcessingStatus;


namespace PodNoms.Common.Services.Downloader {
    public class AudioDownloader {
        private readonly IYouTubeParser _youTubeParser;
        private readonly IRealTimeUpdater _clientUpdater;
        private readonly IDownloader _downloader;
        private readonly ILogger<AudioDownloader> _logger;

        private static readonly List<string> _audioFileTypes = new List<string>() {
            "audio/mpeg"
        };

        public RemoteVideoInfo Properties { get; set; }
        public VideoData RawProperties { get; private set; }

        private readonly HelpersSettings _helpersSettings;

        public event EventHandler<ProcessingProgress> DownloadProgress;

        public AudioDownloader(IYouTubeParser youTubeParser, IOptions<HelpersSettings> helpersSettings,
            IRealTimeUpdater clientUpdater, IDownloader downloader, ILogger<AudioDownloader> logger) {
            _helpersSettings = helpersSettings.Value;
            _youTubeParser = youTubeParser;
            _clientUpdater = clientUpdater;
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

        public async Task<string> DownloadAudio(string id, string url, string userId, string outputFile = "") {
            if (string.IsNullOrEmpty(outputFile)) {
                outputFile = Path.Combine(Path.GetTempPath(), $"{id}.mp3");
            }

            if (await _remoteIsAudio(url)) {
                return _downloadFileDirect(url, outputFile);
            }


            _logger.LogInformation(
                $"Initiating download of ${url}\n\tTo: {outputFile}\n\tUsing: {_helpersSettings.Downloader}");

            async Task<bool> ProgressCallback(ProcessingProgress progress) {
                var result = await _clientUpdater.SendProcessUpdate(userId, id, progress);
                return result;
            }

            outputFile = await _downloader.DownloadFromUrl(url, outputFile, new Dictionary<string, string> {
                {"Downloader", _helpersSettings.Downloader},
                {"FFMPeg", _helpersSettings.FFMPeg}
            }, string.IsNullOrEmpty(userId) ? null : ProgressCallback);
            return File.Exists(outputFile) ? outputFile : string.Empty;
        }

        private string _downloadFileDirect(string url, string fileName) {
            var file = HttpUtils.DownloadFile(url, fileName).WaitAndUnwrapException();
            return file;
        }
    }
}
