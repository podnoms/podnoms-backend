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
    ///     Object containing PostProcessing parameters
    /// </summary>
    public class PostProcessing : OptionSection
    {
        [Option] internal readonly BoolOption addMetadata = new BoolOption("--add-metadata");

        [Option] internal readonly EnumOption<Enums.AudioFormat> audioFormat =
            new EnumOption<Enums.AudioFormat>("--audio-format");

        [Option] internal readonly StringOption audioQuality = new StringOption("--audio-quality");

        [Option] internal readonly StringOption command = new StringOption("--exec");

        [Option] internal readonly EnumOption<Enums.SubtitleFormat> convertSubs =
            new EnumOption<Enums.SubtitleFormat>("--convert-subs");

        [Option] internal readonly BoolOption embedSubs = new BoolOption("--embed-subs");

        [Option] internal readonly BoolOption embedThumbnail = new BoolOption("--embed-thumbnail");

        [Option] internal readonly BoolOption extractAudio = new BoolOption("-x");

        [Option] internal readonly StringOption ffmpegLocation = new StringOption("--ffmpeg-location");

        [Option] internal readonly EnumOption<Enums.FixupPolicy> fixupPolicy =
            new EnumOption<Enums.FixupPolicy>("--fixup");

        [Option] internal readonly BoolOption keepVideo = new BoolOption("-k");

        [Option] internal readonly StringOption metadataFromTitle = new StringOption("--metadata-from-title");

        [Option] internal readonly BoolOption noPostOverwrites = new BoolOption("--no-post-overwrites");

        [Option] internal readonly StringOption postProcessorArgs = new StringOption("--postprocessor-args");

        [Option] internal readonly BoolOption preferAvconv = new BoolOption("--prefer-avconv");

        [Option] internal readonly BoolOption preferFfmpeg = new BoolOption("--prefer-ffmpeg");

        [Option] internal readonly EnumOption<Enums.VideoFormat> recodeFormat =
            new EnumOption<Enums.VideoFormat>("--recode-video");

        [Option] internal readonly BoolOption xattrs = new BoolOption("--xattrs");

        /// <summary>
        ///     --add-metadata
        /// </summary>
        public bool AddMetadata
        {
            get => addMetadata.Value ?? false;
            set => SetField(ref addMetadata.Value, value);
        }

        /// <summary>
        ///     --audio-format
        /// </summary>
        public Enums.AudioFormat AudioFormat
        {
            get => audioFormat.Value is null
                ? Enums.AudioFormat.best
                : (Enums.AudioFormat) audioFormat.Value;
            set => SetField(ref audioFormat.Value, (int) value);
        }

        /// <summary>
        ///     --audio-quality
        /// </summary>
        public string AudioQuality
        {
            get => audioQuality.Value ?? "5";
            set => SetField(ref audioQuality.Value, value);
        }

        /// <summary>
        ///     --exec
        /// </summary>
        public string Command
        {
            get => command.Value;
            set => SetField(ref command.Value, value);
        }

        /// <summary>
        ///     --convert-subs
        /// </summary>
        public Enums.SubtitleFormat ConvertSubs
        {
            get => convertSubs.Value is null
                ? Enums.SubtitleFormat.undefined
                : (Enums.SubtitleFormat) convertSubs.Value;
            set => SetField(ref convertSubs.Value, (int) value);
        }

        /// <summary>
        ///     --embed-subs
        /// </summary>
        public bool EmbedSubs
        {
            get => embedSubs.Value ?? false;
            set => SetField(ref embedSubs.Value, value);
        }

        /// <summary>
        ///     --embed-thumbnail
        /// </summary>
        public bool EmbedThumbnail
        {
            get => embedThumbnail.Value ?? false;
            set => SetField(ref embedThumbnail.Value, value);
        }

        /// <summary>
        ///     -x
        /// </summary>
        public bool ExtractAudio
        {
            get => extractAudio.Value ?? false;
            set => SetField(ref extractAudio.Value, value);
        }

        /// <summary>
        ///     --ffmpeg-location
        /// </summary>
        public string FfmpegLocation
        {
            get => ffmpegLocation.Value;
            set => SetField(ref ffmpegLocation.Value, value);
        }

        /// <summary>
        ///     --fixup
        /// </summary>
        public Enums.FixupPolicy FixupPolicy
        {
            get => fixupPolicy.Value is null
                ? Enums.FixupPolicy.detect_or_warn
                : (Enums.FixupPolicy) fixupPolicy.Value;
            set => SetField(ref fixupPolicy.Value, (int) value);
        }

        /// <summary>
        ///     -k
        /// </summary>
        public bool KeepVideo
        {
            get => keepVideo.Value ?? false;
            set => SetField(ref keepVideo.Value, value);
        }

        /// <summary>
        ///     --metadata-from-title
        /// </summary>
        public string MetadataFromTitle
        {
            get => metadataFromTitle.Value;
            set => SetField(ref metadataFromTitle.Value, value);
        }

        /// <summary>
        ///     --no-post-overwrites
        /// </summary>
        public bool NoPostOverwrites
        {
            get => noPostOverwrites.Value ?? false;
            set => SetField(ref noPostOverwrites.Value, value);
        }

        /// <summary>
        ///     --postprocessor-args
        /// </summary>
        public string PostProcessorArgs
        {
            get => postProcessorArgs.Value;
            set => SetField(ref postProcessorArgs.Value, value);
        }

        /// <summary>
        ///     --prefer-avconv
        /// </summary>
        public bool PreferAvconv
        {
            get => preferAvconv.Value ?? false;
            set => SetField(ref preferAvconv.Value, value);
        }

        /// <summary>
        ///     --prefer-ffmpeg
        /// </summary>
        public bool PreferFfmpeg
        {
            get => preferFfmpeg.Value ?? false;
            set => SetField(ref preferFfmpeg.Value, value);
        }

        /// <summary>
        ///     --recode-video
        /// </summary>
        public Enums.VideoFormat RecodeFormat
        {
            get => recodeFormat.Value is null
                ? Enums.VideoFormat.undefined
                : (Enums.VideoFormat) recodeFormat.Value;
            set => SetField(ref recodeFormat.Value, (int) value);
        }

        /// <summary>
        ///     [Experimental]
        ///     --xattrs
        /// </summary>
        public bool Xattrs
        {
            get => xattrs.Value ?? false;
            set => SetField(ref xattrs.Value, value);
        }
    }
}