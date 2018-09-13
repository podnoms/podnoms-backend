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
    ///     Object containing Verbosity and Simulation parameters
    /// </summary>
    public class VerbositySimulation : OptionSection
    {
        [Option] internal readonly BoolOption callHome = new BoolOption("-C");

        [Option] internal readonly BoolOption consoleTitle = new BoolOption("--console-title");

        [Option] internal readonly BoolOption dumpJson = new BoolOption("-j");

        [Option] internal readonly BoolOption dumpPages = new BoolOption("--dump-pages");

        [Option] internal readonly BoolOption dumpSingleJson = new BoolOption("-J");

        [Option] internal readonly BoolOption getDescription = new BoolOption("--get-description");

        [Option] internal readonly BoolOption getDuration = new BoolOption("--get-duration");

        [Option] internal readonly BoolOption getFilename = new BoolOption("--get-filename");

        [Option] internal readonly BoolOption getFormat = new BoolOption("--get-format");

        [Option] internal readonly BoolOption getId = new BoolOption("--get-id");

        [Option] internal readonly BoolOption getThumbnail = new BoolOption("--get-thumbnail");

        [Option] internal readonly BoolOption getTitle = new BoolOption("-e");

        [Option] internal readonly BoolOption getUrl = new BoolOption("-g");

        [Option] internal readonly BoolOption newline = new BoolOption("--newline");

        [Option] internal readonly BoolOption noCallHome = new BoolOption("--no-call-home");

        [Option] internal readonly BoolOption noProgress = new BoolOption("--no-progress");

        [Option] internal readonly BoolOption noWarnings = new BoolOption("--no-warnings");

        [Option] internal readonly BoolOption printJson = new BoolOption("--print-jobs");

        [Option] internal readonly BoolOption printTraffic = new BoolOption("--print-traffic");

        [Option] internal readonly BoolOption quiet = new BoolOption("-q");

        [Option] internal readonly BoolOption simulate = new BoolOption("-s");

        [Option] internal readonly BoolOption skipDownload = new BoolOption("--skip-download");

        [Option] internal readonly BoolOption verbose = new BoolOption("-v");

        [Option] internal readonly BoolOption writePages = new BoolOption("--write-pages");

        /// <summary>
        ///     -C
        /// </summary>
        public bool CallHome
        {
            get => callHome.Value ?? false;
            set => SetField(ref callHome.Value, value);
        }

        /// <summary>
        ///     --console-title
        /// </summary>
        public bool ConsoleTitle
        {
            get => consoleTitle.Value ?? false;
            set => SetField(ref consoleTitle.Value, value);
        }

        /// <summary>
        ///     -j
        /// </summary>
        public bool DumpJson
        {
            get => dumpJson.Value ?? false;
            set => SetField(ref dumpJson.Value, value);
        }

        /// <summary>
        ///     --dump-pages
        /// </summary>
        public bool DumpPages
        {
            get => dumpPages.Value ?? false;
            set => SetField(ref dumpPages.Value, value);
        }

        /// <summary>
        ///     -J
        /// </summary>
        public bool DumpSingleJson
        {
            get => dumpSingleJson.Value ?? false;
            set => SetField(ref dumpSingleJson.Value, value);
        }

        /// <summary>
        ///     --get-description
        /// </summary>
        public bool GetDescription
        {
            get => getDescription.Value ?? false;
            set => SetField(ref getDescription.Value, value);
        }

        /// <summary>
        ///     --get-duration
        /// </summary>
        public bool GetDuration
        {
            get => getDuration.Value ?? false;
            set => SetField(ref getDuration.Value, value);
        }

        /// <summary>
        ///     --get-filename
        /// </summary>
        public bool GetFilename
        {
            get => getFilename.Value ?? false;
            set => SetField(ref getFilename.Value, value);
        }

        /// <summary>
        ///     --get-format
        /// </summary>
        public bool GetFormat
        {
            get => getFormat.Value ?? false;
            set => SetField(ref getFormat.Value, value);
        }

        /// <summary>
        ///     --get-id
        /// </summary>
        public bool GetId
        {
            get => getId.Value ?? false;
            set => SetField(ref getId.Value, value);
        }

        /// <summary>
        ///     --get-thumbnail
        /// </summary>
        public bool GetThumbnail
        {
            get => getThumbnail.Value ?? false;
            set => SetField(ref getThumbnail.Value, value);
        }

        /// <summary>
        ///     -e
        /// </summary>
        public bool GetTitle
        {
            get => getTitle.Value ?? false;
            set => SetField(ref getTitle.Value, value);
        }

        /// <summary>
        ///     -g
        /// </summary>
        public bool GetUrl
        {
            get => getUrl.Value ?? false;
            set => SetField(ref getUrl.Value, value);
        }

        /// <summary>
        ///     --newline
        /// </summary>
        public bool Newline
        {
            get => newline.Value ?? false;
            set => SetField(ref newline.Value, value);
        }

        /// <summary>
        ///     --no-call-home
        /// </summary>
        public bool NoCallHome
        {
            get => noCallHome.Value ?? false;
            set => SetField(ref noCallHome.Value, value);
        }

        /// <summary>
        ///     --no-progress
        /// </summary>
        public bool NoProgress
        {
            get => noProgress.Value ?? false;
            set => SetField(ref noProgress.Value, value);
        }

        /// <summary>
        ///     --no-warnings
        /// </summary>
        public bool NoWarnings
        {
            get => noWarnings.Value ?? false;
            set => SetField(ref noWarnings.Value, value);
        }

        /// <summary>
        ///     --print-json
        /// </summary>
        public bool PrintJson
        {
            get => printJson.Value ?? false;
            set => SetField(ref printJson.Value, value);
        }

        /// <summary>
        ///     --print-traffic
        /// </summary>
        public bool PrintTraffic
        {
            get => printTraffic.Value ?? false;
            set => SetField(ref printTraffic.Value, value);
        }

        /// <summary>
        ///     -q
        /// </summary>
        public bool Quiet
        {
            get => quiet.Value ?? false;
            set => SetField(ref quiet.Value, value);
        }

        /// <summary>
        ///     -s
        /// </summary>
        public bool Simulate
        {
            get => simulate.Value ?? false;
            set => SetField(ref simulate.Value, value);
        }

        /// <summary>
        ///     --skip-download
        /// </summary>
        public bool SkipDownload
        {
            get => skipDownload.Value ?? false;
            set => SetField(ref skipDownload.Value, value);
        }

        /// <summary>
        ///     -v
        /// </summary>
        public bool Verbose
        {
            get => verbose.Value ?? false;
            set => SetField(ref verbose.Value, value);
        }

        /// <summary>
        ///     --write-pages
        /// </summary>
        public bool WritePages
        {
            get => writePages.Value ?? false;
            set => SetField(ref writePages.Value, value);
        }
    }
}