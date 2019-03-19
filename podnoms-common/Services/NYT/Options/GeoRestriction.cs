﻿// Copyright 2017 Brian Allred
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

    public class GeoRestriction : OptionSection
    {
        [Option] internal readonly BoolOption geoBypass = new BoolOption("--geo-bypass");

        [Option] internal readonly StringOption geoBypassCountry = new StringOption("--geo-bypass-country");

        [Option] internal readonly StringOption geoVerificationProxy = new StringOption("--geo-verification-proxy");

        [Option] internal readonly BoolOption noGeoBypass = new BoolOption("--no-geo-bypass");

        /// <summary>
        ///     --geo-verification-proxy
        /// </summary>
        public string GeoVerificationProxy
        {
            get => geoVerificationProxy.Value;
            set => SetField(ref geoVerificationProxy.Value, value);
        }

        /// <summary>
        ///     --geo-bypass
        /// </summary>
        public bool GeoBypass
        {
            get => geoBypass.Value ?? false;
            set => SetField(ref geoBypass.Value, value);
        }

        /// <summary>
        ///     --geo-bypass
        /// </summary>
        public bool NoGeoBypass
        {
            get => noGeoBypass.Value ?? false;
            set => SetField(ref noGeoBypass.Value, value);
        }

        /// <summary>
        ///     --geo-bypass-country
        /// </summary>
        public string GeoBypassCountry
        {
            get => geoBypassCountry.Value;
            set => SetField(ref geoBypassCountry.Value, value);
        }
    }
}