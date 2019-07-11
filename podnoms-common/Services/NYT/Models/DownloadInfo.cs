// Copyright 2017 Brian Allred
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to
// deal in the Software without restriction, including without limitation the
// rights to use, copy, modify, merge, publish, distribute, sublicense, and/or
// sell copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PodNoms.Common.Services.NYT.Helpers;

namespace PodNoms.Common.Services.NYT.Models {
    #region Using

    #endregion

    /// <summary>
    ///     Class holding data about the current download, which is parsed from youtube-dl's standard output
    /// </summary>
    public class DownloadInfo : NotifyPropertyChangedEx {
        protected const string ALREADY = "already";

        protected const string DOWNLOADRATESTRING = "iB/s";

        protected const string DOWNLOADSIZESTRING = "iB";

        protected const string ETASTRING = "ETA";

        protected const string OFSTRING = "of";

        protected const string VIDEOSTRING = "video";

        private string downloadRate;

        private string eta;

        private string status = Enums.DownloadStatus.WAITING.ToString();
        private string id;
        private string title;

        private int videoProgress;

        private string videoSize;

        /// <summary>
        ///     The current download rate
        /// </summary>
        public string DownloadRate {
            get => downloadRate;
            set => SetField(ref downloadRate, value);
        }

        /// <summary>
        ///     The collection of error messages received
        /// </summary>
        public List<string> Errors { get; } = new List<string>();

        /// <summary>
        ///     The current download's estimated time remaining
        /// </summary>
        public string Eta {
            get => eta;
            set => SetField(ref eta, value);
        }

        /// <summary>
        ///     The status of the video currently downloading
        /// </summary>
        public string Status {
            get => status;
            set {
                if (!status.Equals(Enums.DownloadStatus.ERROR.ToString()) &&
                    !status.Equals(Enums.DownloadStatus.WARNING.ToString())) {
                    SetField(ref status, value);
                } else if (value.Equals(Enums.DownloadStatus.ERROR.ToString()) &&
                           status.Equals(Enums.DownloadStatus.WARNING.ToString())) {
                    SetField(ref status, value);
                }
            }
        }

        /// <summary>
        ///     The id of the playlist
        /// </summary>
        public string Id {
            get => id;
            set => SetField(ref id, value);
        }
        /// <summary>
        ///     The title of the video currently downloading
        /// </summary>
        public string Title {
            get => title;
            set => SetField(ref title, value);
        }

        /// <summary>
        ///     The current download progresss
        /// </summary>
        public int VideoProgress {
            get => videoProgress;
            set {
                SetField(ref videoProgress, value);

                if (value == 0) {
                    Status = Enums.DownloadStatus.WAITING.ToString();
                } else if (value == 100) {
                    Status = Enums.DownloadStatus.DONE.ToString();
                } else {
                    Status = Enums.DownloadStatus.DOWNLOADING.ToString();
                }
            }
        }

        /// <summary>
        ///     The current download's total size
        /// </summary>
        public string VideoSize {
            get => videoSize;
            set => SetField(ref videoSize, value);
        }

        /// <summary>
        ///     The collection of warning messages received
        /// </summary>
        public List<string> Warnings { get; } = new List<string>();

        internal static DownloadInfo CreateDownloadInfo(string output) {
            if (string.IsNullOrEmpty(output) || output.Equals("null"))
                return null;

            try {
                var info = JsonConvert.DeserializeObject<PlaylistInfo>(output);
                if (!string.IsNullOrEmpty(info._type) && info._type.Equals("playlist")) {
                    return new PlaylistDownloadInfo(info);
                }
            } catch (Exception ex) {
                Console.WriteLine(ex);
            }

            try {
                var info = JsonConvert.DeserializeObject<VideoInfo>(output);
                if (!string.IsNullOrEmpty(info.title)) {
                    return new VideoDownloadInfo(info);
                }
            } catch (Exception ex) {
                Console.WriteLine(ex);
            }

            return null;
        }

        /// <summary>
        ///     Fired when an error occurs
        /// </summary>
        public event EventHandler<string> ErrorEvent;

        internal virtual void ParseError(object sender, string error) {
            ErrorEvent?.Invoke(this, error);
            if (error.Contains("WARNING")) {
                Warnings.Add(error);
                Status = Enums.DownloadStatus.WARNING.ToString();
            } else if (error.Contains("ERROR")) {
                Errors.Add(error);
                Status = Enums.DownloadStatus.ERROR.ToString();
            }
        }

        internal virtual void ParseOutput(object sender, string output) {
            try {
                if (output.Contains("%")) {
                    var progressIndex = output.LastIndexOf(' ', output.IndexOf('%')) + 1;
                    var progressString = output.Substring(progressIndex, output.IndexOf('%') - progressIndex);
                    VideoProgress = (int)Math.Round(double.Parse(progressString));

                    var sizeIndex = output.LastIndexOf(' ', output.IndexOf(DOWNLOADSIZESTRING)) + 1;
                    var sizeString = output.Substring(sizeIndex, output.IndexOf(DOWNLOADSIZESTRING) - sizeIndex + 2);
                    VideoSize = sizeString;
                }

                if (output.Contains(DOWNLOADRATESTRING)) {
                    var rateIndex = output.LastIndexOf(' ', output.LastIndexOf(DOWNLOADRATESTRING)) + 1;
                    var rateString =
                        output.Substring(rateIndex, output.LastIndexOf(DOWNLOADRATESTRING) - rateIndex + 4);
                    DownloadRate = rateString;
                }

                if (output.Contains(ETASTRING)) {
                    Eta = output.Substring(output.LastIndexOf(' ') + 1);
                }

                if (output.Contains(ALREADY)) {
                    Status = Enums.DownloadStatus.DONE.ToString();
                    VideoProgress = 100;
                }
            } catch (Exception) {
            }
        }
    }
}
