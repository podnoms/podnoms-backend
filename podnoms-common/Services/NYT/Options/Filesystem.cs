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

using System.Linq;
using PodNoms.Common.Services.NYT.Helpers;

namespace PodNoms.Common.Services.NYT.Options
{
    #region Using

    #endregion

    /// <summary>
    ///     Object containing Filesystem parameters
    /// </summary>
    public class Filesystem : OptionSection
    {
        [Option] internal readonly IntOption autoNumberSize = new IntOption("--autonumber-size");

        [Option] internal readonly IntOption autoNumberStart = new IntOption("--autonumber-start");

        [Option] internal readonly StringOption batchFile = new StringOption("-a");

        [Option] internal readonly StringOption cacheDir = new StringOption("--cache-dir");

        [Option] internal readonly BoolOption continueOpt = new BoolOption("-C");

        [Option] internal readonly StringOption cookies = new StringOption("--cookies");

        [Option] internal readonly BoolOption id = new BoolOption("--id");

        [Option] internal readonly StringOption loadInfoJson = new StringOption("--load-info-json");

        [Option] internal readonly BoolOption noCacheDir = new BoolOption("--no-cache-dir");

        [Option] internal readonly BoolOption noContinue = new BoolOption("--no-continue");

        [Option] internal readonly BoolOption noMtime = new BoolOption("--no-mtime");

        [Option] internal readonly BoolOption noOverwrites = new BoolOption("-w");

        [Option] internal readonly BoolOption noPart = new BoolOption("--no-part");

        [Option] internal readonly StringOption output = new StringOption("-o");

        [Option] internal readonly BoolOption restrictFilenames = new BoolOption("--restrict-filenames");

        [Option] internal readonly BoolOption rmCacheDir = new BoolOption("--rm-cache-dir");

        [Option] internal readonly BoolOption writeAnnotations = new BoolOption("--write-annotations");

        [Option] internal readonly BoolOption writeDescription = new BoolOption("--write-desription");

        [Option] internal readonly BoolOption writeInfoJson = new BoolOption("--write-info-json");

        /// <summary>
        ///     --autonumber-size
        /// </summary>
        public int AutoNumberSize
        {
            get => autoNumberSize.Value ?? 5;
            set => SetField(ref autoNumberSize.Value, value);
        }

        /// <summary>
        ///     --autonumber-start
        /// </summary>
        public int AutoNumberStart
        {
            get => autoNumberStart.Value ?? 1;
            set => SetField(ref autoNumberStart.Value, value);
        }

        /// <summary>
        ///     -a
        /// </summary>
        public string BatchFile
        {
            get => batchFile.Value;
            set => SetField(ref batchFile.Value, value);
        }

        /// <summary>
        ///     --cache-dir
        /// </summary>
        public string CacheDir
        {
            get => cacheDir.Value;
            set => SetField(ref cacheDir.Value, value);
        }

        /// <summary>
        ///     -c
        /// </summary>
        public bool Continue
        {
            get => continueOpt.Value ?? false;
            set => SetField(ref continueOpt.Value, value);
        }

        /// <summary>
        ///     --cookies
        /// </summary>
        public string Cookies
        {
            get => cookies.Value;
            set => SetField(ref cookies.Value, value);
        }

        /// <summary>
        ///     --id
        /// </summary>
        public bool Id
        {
            get => id.Value ?? false;
            set => SetField(ref id.Value, value);
        }

        /// <summary>
        ///     --load-info-json
        /// </summary>
        public string LoadInfoJson
        {
            get => loadInfoJson.Value;
            set => SetField(ref loadInfoJson.Value, value);
        }

        /// <summary>
        ///     --no-cache-dir
        /// </summary>
        public bool NoCacheDir
        {
            get => noCacheDir.Value ?? false;
            set => SetField(ref noCacheDir.Value, value);
        }

        /// <summary>
        ///     --no-continue
        /// </summary>
        public bool NoContinue
        {
            get => noContinue.Value ?? false;
            set => SetField(ref noContinue.Value, value);
        }

        /// <summary>
        ///     --no-mtime
        /// </summary>
        public bool NoMtime
        {
            get => noMtime.Value ?? false;
            set => SetField(ref noMtime.Value, value);
        }

        /// <summary>
        ///     -w
        /// </summary>
        public bool NoOverwrites
        {
            get => noOverwrites.Value ?? false;
            set => SetField(ref noOverwrites.Value, value);
        }

        /// <summary>
        ///     --no-part
        /// </summary>
        public bool NoPart
        {
            get => noPart.Value ?? false;
            set => SetField(ref noPart.Value, value);
        }

        /// <summary>
        ///     -o
        /// </summary>
        public string Output
        {
            get => output.Value;
            set => SetField(ref output.Value, value);
        }

        /// <summary>
        ///     --restrict-filenames
        /// </summary>
        public bool RestrictFilenames
        {
            get => restrictFilenames.Value ?? false;
            set => SetField(ref restrictFilenames.Value, value);
        }

        /// <summary>
        ///     --rm-cache-dir
        /// </summary>
        public bool RmCacheDir
        {
            get => rmCacheDir.Value ?? false;
            set => SetField(ref rmCacheDir.Value, value);
        }

        /// <summary>
        ///     --write-annotations
        /// </summary>
        public bool WriteAnnotations
        {
            get => writeAnnotations.Value ?? false;
            set => SetField(ref writeAnnotations.Value, value);
        }

        /// <summary>
        ///     --write-description
        /// </summary>
        public bool WriteDescription
        {
            get => writeDescription.Value ?? false;
            set => SetField(ref writeDescription.Value, value);
        }

        /// <summary>
        ///     --write-info-json
        /// </summary>
        public bool WriteInfoJson
        {
            get => writeInfoJson.Value ?? false;
            set => SetField(ref writeInfoJson.Value, value);
        }

        /// <summary>
        ///     Retrieves the options from this option section
        /// </summary>
        /// <returns>
        ///     The parameterized string of the options in this section
        /// </returns>
        public override string ToCliParameters()
        {
            if (output?.Value != null && output.Value.Any(char.IsWhiteSpace))
            {
                output.Value = $"\"{output.Value}\"";
            }

            return base.ToCliParameters();
        }
    }
}