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
    ///     Object containing Video Format parameters
    /// </summary>
    public class VideoFormat : OptionSection
    {
        [Option] internal readonly BoolOption allFormats = new BoolOption("--all-formats");

        [Option] internal readonly EnumOption<Enums.VideoFormat> format = new EnumOption<Enums.VideoFormat>("-f");

        [Option] internal readonly StringOption formatAdvanced = new StringOption("-f");

        [Option] internal readonly BoolOption listFormats = new BoolOption("-F");

        [Option] internal readonly EnumOption<Enums.VideoFormat> mergeOutputFormat =
            new EnumOption<Enums.VideoFormat>("--merge-output-format");

        [Option] internal readonly BoolOption preferFreeFormats = new BoolOption("--prefer-free-formats");

        [Option] internal readonly BoolOption youtubeSkipDashManifest = new BoolOption("--youtube-skip-dash-manifest");

        /// <summary>
        ///     --all-formats
        /// </summary>
        public bool AllFormats
        {
            get => allFormats.Value ?? false;
            set => SetField(ref allFormats.Value, value);
        }

        /// <summary>
        ///     This is a simple version of -f. For more advanced format usage, use the FormatAdvanced
        ///     property.
        ///     NOTE: FormatAdvanced takes precedence over Format.
        /// </summary>
        public Enums.VideoFormat Format
        {
            get => format.Value == null ? Enums.VideoFormat.undefined : (Enums.VideoFormat) format.Value;
            set => SetField(ref format.Value, (int) value);
        }

        /// <summary>
        ///     This accepts a string matching -f according to the youtube-dl documentation below.
        ///     NOTE: FormatAdvanced takes precedence over Format.
        ///     <see cref="https://github.com/rg3/youtube-dl/blob/master/README.md#format-selection" />
        /// </summary>
        public string FormatAdvanced
        {
            get => formatAdvanced.Value;
            set => SetField(ref formatAdvanced.Value, value);
        }

        /// <summary>
        ///     -F
        /// </summary>
        public bool ListFormats
        {
            get => listFormats.Value ?? false;
            set => SetField(ref listFormats.Value, value);
        }

        /// <summary>
        ///     --merge-output-format
        /// </summary>
        public Enums.VideoFormat MergeOutputFormat
        {
            get => mergeOutputFormat.Value == null
                ? Enums.VideoFormat.undefined
                : (Enums.VideoFormat) mergeOutputFormat.Value;
            set => SetField(ref mergeOutputFormat.Value, (int) value);
        }

        /// <summary>
        ///     --prefer-free-formats
        /// </summary>
        public bool PreferFreeFormats
        {
            get => preferFreeFormats.Value ?? false;
            set => SetField(ref preferFreeFormats.Value, value);
        }

        /// <summary>
        ///     --youtube-skip-dash-manifest
        /// </summary>
        public bool YoutubeSkipDashManifest
        {
            get => youtubeSkipDashManifest.Value ?? false;
            set => SetField(ref youtubeSkipDashManifest.Value, value);
        }

        public override string ToCliParameters()
        {
            // Set format to undefined if formatAdvanced has a valid value,
            // then return the parameters.
            if (!string.IsNullOrWhiteSpace(formatAdvanced.Value))
            {
                format.Value = (int) Enums.VideoFormat.undefined;
            }

            return base.ToCliParameters();
        }
    }
}