using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Nito.AsyncEx.Synchronous;
using PodNoms.Common.Data.ViewModels;
using PodNoms.Common.Services.NYT;
using PodNoms.Common.Services.NYT.Helpers;
using PodNoms.Common.Services.NYT.Models;
using PodNoms.Common.Utils;
using PodNoms.Common.Utils.RemoteParsers;

namespace PodNoms.Common.Services.Downloader {
    public class AudioDownloader {
        private readonly string _url;
        private readonly string _downloader;

        public VideoDownloadInfo Properties => RawProperties is VideoDownloadInfo info ? info : null;
        public DownloadInfo RawProperties { get; private set; }

        private const string DOWNLOADRATESTRING = "iB/s";
        private const string DOWNLOADSIZESTRING = "iB";
        private const string ETASTRING = "ETA";
        protected const string OFSTRING = "of";

        public event EventHandler<ProcessProgressEvent> DownloadProgress;
        public event EventHandler<string> PostProcessing;

        public AudioDownloader(string url, string downloader) {
            _url = url;
            _downloader = downloader;
        }

        public static string GetVersion(string downloader) {
            try {
                var proc = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
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

        public AudioType GetInfo() {
            var ret = AudioType.Invalid;

            if (_url.Contains("drive.google.com")) {
                return AudioType.Valid;
            }

            var yt = new YoutubeDL { VideoUrl = _url };
            var info = yt.GetDownloadInfo();

            if (info is null ||
                info.Errors.Count != 0 ||
                (info.GetType() == typeof(PlaylistDownloadInfo) && 
                    !MixcloudParser.ValidateUrl(_url) && 
                    !YouTubeParser.ValidateUrl(_url))) {
                return ret; 
            }
            RawProperties = info;
            switch (info) {
                // have to dump playlist handling for now
                case PlaylistDownloadInfo _ when ((PlaylistDownloadInfo)info).Videos.Count > 0:
                    ret = AudioType.Playlist;
                    break;
                case VideoDownloadInfo _:
                    ret = AudioType.Valid;
                    break;
            }

            return ret;
        }

        public string DownloadAudio(Guid id) {
            var outputFile = Path.Combine(Path.GetTempPath(), $"{id}.mp3");
            var templateFile = Path.Combine(Path.GetTempPath(), $"{id}.%(ext)s");

            if (_url.Contains("drive.google.com")) {
                return _downloadFileDirect(_url, outputFile);
            }

            var yt = new YoutubeDL();
            yt.Options.FilesystemOptions.Output = templateFile;
            yt.Options.PostProcessingOptions.ExtractAudio = true;
            yt.Options.PostProcessingOptions.AudioFormat = Enums.AudioFormat.mp3;

            yt.VideoUrl = _url;

            yt.StandardOutputEvent += (sender, output) => {
                if (output.Contains("%")) {
                    var progress = _parseProgress(output);
                    DownloadProgress?.Invoke(this, progress);
                } else {
                    Console.WriteLine(output);
                    PostProcessing?.Invoke(this, output);
                }
            };
            yt.PrepareDownload();

            Console.WriteLine(yt.RunCommand);

            var yp = yt.Download();
            yp.WaitForExit();
            return File.Exists(outputFile) ? outputFile : string.Empty;
        }

        private ProcessProgressEvent _parseProgress(string output) {
            var result = new ProcessProgressEvent();

            var progressIndex = output.LastIndexOf(' ', output.IndexOf('%')) + 1;
            var progressString = output.Substring(progressIndex, output.IndexOf('%') - progressIndex);
            result.Percentage = (int)Math.Round(double.Parse(progressString));

            var sizeIndex = output.LastIndexOf(' ', output.IndexOf(DOWNLOADSIZESTRING, StringComparison.Ordinal)) + 1;
            var sizeString = output.Substring(sizeIndex,
                output.IndexOf(DOWNLOADSIZESTRING, StringComparison.Ordinal) - sizeIndex + 2);
            result.TotalSize = sizeString;

            if (output.Contains(DOWNLOADRATESTRING)) {
                var rateIndex =
                    output.LastIndexOf(' ', output.LastIndexOf(DOWNLOADRATESTRING, StringComparison.Ordinal)) + 1;
                var rateString = output.Substring(rateIndex,
                    output.LastIndexOf(DOWNLOADRATESTRING, StringComparison.Ordinal) - rateIndex + 4);
                result.CurrentSpeed = rateString;
            }

            if (output.Contains(ETASTRING)) {
                result.ETA = output.Substring(output.LastIndexOf(' ') + 1);
            }

            return result;
        }

        private string _downloadFileDirect(string url, string fileName) {
            var file = HttpUtils.DownloadFile(url, fileName).WaitAndUnwrapException();
            return file;
        }
    }
}