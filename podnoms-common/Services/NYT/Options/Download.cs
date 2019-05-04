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

using PodNoms.Common.Services.NYT.Helpers;

namespace PodNoms.Common.Services.NYT.Options
{
    #region Using

    #endregion

    /// <summary>
    ///     Object containing Download parameters
    /// </summary>
    public class Download : OptionSection
    {
        [Option] internal readonly BoolOption abortOnUnvailableFragment =
            new BoolOption("--abort-on-unavailable-fragment");

        [Option] internal readonly FileSizeRateOption bufferSize = new FileSizeRateOption("--buffer-size");

        [Option] internal readonly EnumOption<Enums.ExternalDownloader> externalDownloader =
            new EnumOption<Enums.ExternalDownloader>("--external-downloader");

        [Option] internal readonly StringOption externalDownloaderArgs = new StringOption("--external-downloader-args");

        [Option] internal readonly IntOption fragmentRetries = new IntOption("--fragment-retries", true);

        [Option] internal readonly BoolOption hlsPreferFfmpeg = new BoolOption("--hls-prefer-ffmpeg");

        [Option] internal readonly BoolOption hlsPreferNative = new BoolOption("--hls-prefer-native");

        [Option] internal readonly BoolOption hlsUseMpegts = new BoolOption("--hls-use-mpegts");

        [Option] internal readonly FileSizeRateOption limitRate = new FileSizeRateOption("-r");

        [Option] internal readonly BoolOption noResizeBuffer = new BoolOption("--no-resize-buffer");

        [Option] internal readonly BoolOption playlistRandom = new BoolOption("--playlist-random");

        [Option] internal readonly BoolOption playlistReverse = new BoolOption("--playlist-reverse");

        [Option] internal readonly IntOption retries = new IntOption("-R", true);

        [Option] internal readonly BoolOption skipUnavailableFragments = new BoolOption("--skip-unavailable-fragments");

        [Option] internal readonly BoolOption xattrSetFilesize = new BoolOption("--xattr-set-filesize");

        /// <summary>
        ///     --abort-on-unavailable-fragment
        /// </summary>
        public bool AbortOnUnavailableFragment
        {
            get => abortOnUnvailableFragment.Value ?? false;
            set => SetField(ref abortOnUnvailableFragment.Value, value);
        }

        /// <summary>
        ///     --buffer-size
        /// </summary>
        public FileSizeRate BufferSize
        {
            get => bufferSize.Value;
            set => SetField(ref bufferSize.Value, value);
        }

        /// <summary>
        ///     --external-downloader
        /// </summary>
        public Enums.ExternalDownloader ExternalDownloader
        {
            get => externalDownloader.Value is null
                ? Enums.ExternalDownloader.undefined
                : (Enums.ExternalDownloader) externalDownloader.Value;
            set => SetField(ref externalDownloader.Value, (int) value);
        }

        /// <summary>
        ///     --external-downloader-args
        /// </summary>
        public string ExternalDownloaderArgs
        {
            get => externalDownloaderArgs.Value;
            set => SetField(ref externalDownloaderArgs.Value, value);
        }

        /// <summary>
        ///     --fragment-retries
        /// </summary>
        public int FragmentRetries
        {
            get => fragmentRetries.Value ?? 10;
            set => SetField(ref fragmentRetries.Value, value);
        }

        /// <summary>
        ///     --hls-prefer-ffmpeg
        /// </summary>
        public bool HlsPreferFfmpeg
        {
            get => hlsPreferFfmpeg.Value ?? false;
            set => SetField(ref hlsPreferFfmpeg.Value, value);
        }

        /// <summary>
        ///     --hls-prefer-native
        /// </summary>
        public bool HlsPreferNative
        {
            get => hlsPreferNative.Value ?? false;
            set => SetField(ref hlsPreferNative.Value, value);
        }

        /// <summary>
        ///     --hls-use-mpegts
        /// </summary>
        public bool HlsUseMpegts
        {
            get => hlsUseMpegts.Value ?? false;
            set => SetField(ref hlsUseMpegts.Value, value);
        }

        /// <summary>
        ///     -r
        /// </summary>
        public FileSizeRate LimitRate
        {
            get => limitRate.Value;
            set => SetField(ref limitRate.Value, value);
        }

        /// <summary>
        ///     --no-resize-buffer
        /// </summary>
        public bool NoResizeBuffer
        {
            get => noResizeBuffer.Value ?? false;
            set => SetField(ref noResizeBuffer.Value, value);
        }

        /// <summary>
        ///     --playlist-random
        /// </summary>
        public bool PlaylistRandom
        {
            get => playlistRandom.Value ?? false;
            set => SetField(ref playlistRandom.Value, value);
        }

        /// <summary>
        ///     --playlist-reverse
        /// </summary>
        public bool PlaylistReverse
        {
            get => playlistReverse.Value ?? false;
            set => SetField(ref playlistReverse.Value, value);
        }

        /// <summary>
        ///     -R
        /// </summary>
        public int Retries
        {
            get => retries.Value ?? 10;
            set => SetField(ref retries.Value, value);
        }

        /// <summary>
        ///     --skip-unavailable-fragments
        /// </summary>
        public bool SkipUnavailableFragments
        {
            get => skipUnavailableFragments.Value ?? false;
            set => SetField(ref skipUnavailableFragments.Value, value);
        }

        /// <summary>
        ///     --xattr-set-filesize
        /// </summary>
        public bool XattrSetFilesize
        {
            get => xattrSetFilesize.Value ?? false;
            set => SetField(ref xattrSetFilesize.Value, value);
        }
    }
}