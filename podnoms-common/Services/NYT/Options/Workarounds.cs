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

using System.Collections.Generic;
using PodNoms.Common.Services.NYT.Helpers;

namespace PodNoms.Common.Services.NYT.Options
{
    #region Using

    #endregion

    /// <summary>
    ///     Object containing Workaround parameters
    /// </summary>
    public class Workarounds : OptionSection
    {
        [Option] internal readonly BoolOption biDiWorkaround = new BoolOption("--bidi-workaround");

        [Option] internal readonly StringOption encoding = new StringOption("--encoding");

        private readonly List<string> headers = new List<string>();

        [Option] internal readonly IntOption maxSleepInterval = new IntOption("--max-sleep-interval");

        [Option] internal readonly BoolOption noCheckCertificate = new BoolOption("--no-check-certificate");

        [Option] internal readonly BoolOption preferInsecure = new BoolOption("--prefer-insecure");

        [Option] internal readonly StringOption referer = new StringOption("--referer");

        [Option] internal readonly IntOption sleepInterval = new IntOption("--sleep-interval");

        [Option] internal readonly StringOption userAgent = new StringOption("--user-agent");

        /// <summary>
        ///     --bidi-workaround
        /// </summary>
        public bool BiDiWorkaround
        {
            get => biDiWorkaround.Value ?? false;
            set => SetField(ref biDiWorkaround.Value, value);
        }

        /// <summary>
        ///     --encoding
        /// </summary>
        public string Encoding
        {
            get => encoding.Value;
            set => SetField(ref encoding.Value, value);
        }

        /// <summary>
        ///     --max-sleep-interval
        /// </summary>
        public int MaxSleepInterval
        {
            get => maxSleepInterval.Value ?? -1;
            set => SetField(ref maxSleepInterval.Value, value);
        }

        /// <summary>
        ///     --no-check-certificate
        /// </summary>
        public bool NoCheckCertificate
        {
            get => noCheckCertificate.Value ?? false;
            set => SetField(ref noCheckCertificate.Value, value);
        }

        /// <summary>
        ///     --prefer-insecure
        /// </summary>
        public bool PreferInsecure
        {
            get => preferInsecure.Value ?? false;
            set => SetField(ref preferInsecure.Value, value);
        }

        /// <summary>
        ///     --referer
        /// </summary>
        public string Referer
        {
            get => referer.Value;
            set => SetField(ref referer.Value, value);
        }

        /// <summary>
        ///     --sleep-interval
        /// </summary>
        public int SleepInterval
        {
            get => sleepInterval.Value ?? -1;
            set => SetField(ref sleepInterval.Value, value);
        }

        /// <summary>
        ///     --user-agent
        /// </summary>
        public string UserAgent
        {
            get => userAgent.Value;
            set => SetField(ref userAgent.Value, value);
        }

        /// <summary>
        ///     --add-header
        /// </summary>
        /// <param name="header">
        ///     FIELD:VALUE pair to add as a header
        /// </param>
        public void AddHeader(string header)
        {
            AddHeader(header, false);
        }

        /// <summary>
        ///     --add-header
        /// </summary>
        /// <param name="header">
        ///     FIELD:VALUE pair to add as a header
        /// </param>
        /// <param name="overwrite">
        ///     Overwrite existing identical header (prevents duplicates)
        /// </param>
        public void AddHeader(string header, bool overwrite)
        {
            if (overwrite)
            {
                headers.Remove(header);
            }

            headers.Add(header);
        }

        public override string ToCliParameters()
        {
            foreach (var header in headers)
            {
                CustomParameters.Add("--add-header");
                CustomParameters.Add(header);
            }

            return base.ToCliParameters();
        }
    }
}