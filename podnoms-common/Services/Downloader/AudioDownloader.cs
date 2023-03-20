using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nito.AsyncEx.Synchronous;
using PodNoms.AudioParsing.Downloaders;
using PodNoms.AudioParsing.ErrorHandling;
using PodNoms.AudioParsing.Helpers;
using PodNoms.AudioParsing.Models;
using PodNoms.Common.Data.Settings;
using PodNoms.Common.Services.Realtime;
using PodNoms.Common.Utils;
using PodNoms.Common.Utils.RemoteParsers;
using YoutubeDLSharp.Metadata;


namespace PodNoms.Common.Services.Downloader {
    public class AudioDownloader {
        private readonly IYouTubeParser _youTubeParser;
        private readonly IRealTimeUpdater _clientUpdater;
        private readonly IDownloader _downloader;
        private readonly ILogger<AudioDownloader> _logger;

        private static readonly List<string> _audioFileTypes = new() {
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

        private static async Task<bool> _remoteIsAudioFileUrl(string url) =>
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
                return $"{{\"Error\": \"{ex.Message}\"}}";
            }
        }

        private async Task<VideoData> __getInfo(string url) {
            try {
                var result = await _downloader.GetVideoInformation(url);
                if (result is not null) {
                    return result;
                }
            } catch (TaskCanceledException) {
                _logger.LogError("Unable to parse url");
            } catch (Exception e) {
                _logger.LogError("Error getting info for {Url}\n\t{Message}", url, e.Message);
            }

            throw new AudioDownloadException("No audio found in that URL");
        }

        public async Task<RemoteUrlType> GetInfo(string url, string userId) {
            var ret = RemoteUrlType.Invalid;
            if (await _remoteIsAudioFileUrl(url)) {
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
                    UploadDate = (info.UploadDate ?? DateTime.Now).ToString(CultureInfo.InvariantCulture).ParseBest(),
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
                outputFile = Path.Combine(PathUtils.GetScopedTempPath(), $"{id}.mp3");
            }

            if (await _remoteIsAudioFileUrl(url)) {
                return await HttpUtils.DownloadFile(url, outputFile);
            }

            _logger.LogInformation(
                "Initiating download of ${Url}\\n\\tTo: {OutputFile}\\n\\tUsing: {HelpersSettingsDownloader}", url,
                outputFile, _helpersSettings.Downloader);

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
    }
}
