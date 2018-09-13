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
using PodNoms.Common.Services.NYT.Helpers;

namespace PodNoms.Common.Services.NYT.Options
{
    #region Using

    #endregion

    /// <summary>
    ///     Object containing Video Selection parameters
    /// </summary>
    public class VideoSelection : OptionSection
    {
        [Option] internal readonly IntOption ageLimit = new IntOption("--age-limit");

        [Option] internal readonly DateTimeOption date = new DateTimeOption("--date");

        [Option] internal readonly DateTimeOption dateAfter = new DateTimeOption("--dateafter");

        [Option] internal readonly DateTimeOption dateBefore = new DateTimeOption("--datebefore");

        [Option] internal readonly StringOption downloadArchive = new StringOption("--download-archive");

        [Option] internal readonly BoolOption includeAds = new BoolOption("--include-ads");

        [Option] internal readonly StringOption matchFilter = new StringOption("--match-filter");

        [Option] internal readonly StringOption matchTitle = new StringOption("--match-title");

        [Option] internal readonly IntOption maxDownloads = new IntOption("--max-downloads");

        [Option] internal readonly FileSizeRateOption maxFileSize = new FileSizeRateOption("--max-filesize");

        [Option] internal readonly IntOption maxViews = new IntOption("--max-views");

        [Option] internal readonly FileSizeRateOption minFileSize = new FileSizeRateOption("--min-filesize");

        [Option] internal readonly IntOption minViews = new IntOption("--min-views");

        [Option] internal readonly BoolOption noPlaylist = new BoolOption("--no-playlist");

        [Option] internal readonly IntOption playlistEnd = new IntOption("--playlist-end");

        [Option] internal readonly StringOption playlistItems = new StringOption("--playlist-items");

        [Option] internal readonly IntOption playlistStart = new IntOption("--playlist-start");

        [Option] internal readonly StringOption rejectTitle = new StringOption("--reject-title");

        [Option] internal readonly BoolOption yesPlaylist = new BoolOption("--yes-playlist");

        /// <summary>
        ///     --age-limit
        /// </summary>
        public int AgeLimit
        {
            get => ageLimit.Value ?? -1;
            set => SetField(ref ageLimit.Value, value);
        }

        /// <summary>
        ///     --date
        /// </summary>
        public DateTime? Date
        {
            get => date.Value;
            set => SetField(ref date.Value, value);
        }

        /// <summary>
        ///     --dateafter
        /// </summary>
        public DateTime? DateAfter
        {
            get => dateAfter.Value;
            set => SetField(ref dateAfter.Value, value);
        }

        /// <summary>
        ///     --datebefore
        /// </summary>
        public DateTime? DateBefore
        {
            get => dateBefore.Value;
            set => SetField(ref dateBefore.Value, value);
        }

        /// <summary>
        ///     --download-archive
        /// </summary>
        public string DownloadArchive
        {
            get => downloadArchive.Value;
            set => SetField(ref downloadArchive.Value, value);
        }

        /// <summary>
        ///     [Experimental]
        ///     --include-ads
        /// </summary>
        public bool IncludeAds
        {
            get => includeAds.Value ?? false;
            set => SetField(ref includeAds.Value, value);
        }

        /// <summary>
        ///     --match-filter
        /// </summary>
        public string MatchFilter
        {
            get => matchFilter.Value;
            set => SetField(ref matchFilter.Value, value);
        }

        /// <summary>
        ///     --match-title
        /// </summary>
        public string MatchTitle
        {
            get => matchTitle.Value;
            set => SetField(ref matchTitle.Value, value);
        }

        /// <summary>
        ///     --max-downloads
        /// </summary>
        public int MaxDownloads
        {
            get => maxDownloads.Value ?? -1;
            set => SetField(ref maxDownloads.Value, value);
        }

        /// <summary>
        ///     --max-filesize
        /// </summary>
        public FileSizeRate MaxFileSize
        {
            get => maxFileSize.Value;
            set => SetField(ref maxFileSize.Value, value);
        }

        /// <summary>
        ///     --max-views
        /// </summary>
        public int MaxViews
        {
            get => maxViews.Value ?? -1;
            set => SetField(ref maxViews.Value, value);
        }

        /// <summary>
        ///     --min-filesize
        /// </summary>
        public FileSizeRate MinFileSize
        {
            get => minFileSize.Value;
            set => SetField(ref minFileSize.Value, value);
        }

        /// <summary>
        ///     --min-views
        /// </summary>
        public int MinViews
        {
            get => minViews.Value ?? -1;
            set => SetField(ref minViews.Value, value);
        }

        /// <summary>
        ///     --no-playlist
        /// </summary>
        public bool NoPlaylist
        {
            get => noPlaylist.Value ?? false;
            set => SetField(ref noPlaylist.Value, value);
        }

        /// <summary>
        ///     --playlist-end
        /// </summary>
        public int PlaylistEnd
        {
            get => playlistEnd.Value ?? -1;
            set => SetField(ref playlistEnd.Value, value);
        }

        /// <summary>
        ///     --playlist-items
        /// </summary>
        public string PlaylistItems
        {
            get => playlistItems.Value;
            set => SetField(ref playlistItems.Value, value);
        }

        /// <summary>
        ///     --playlist-start
        /// </summary>
        public int PlaylistStart
        {
            get => playlistStart.Value ?? 1;
            set => SetField(ref playlistStart.Value, value);
        }

        /// <summary>
        ///     --reject-title
        /// </summary>
        public string RejectTitle
        {
            get => rejectTitle.Value;
            set => SetField(ref rejectTitle.Value, value);
        }

        /// <summary>
        ///     --yes-playlist
        /// </summary>
        public bool YesPlaylist
        {
            get => yesPlaylist.Value ?? false;
            set => SetField(ref yesPlaylist.Value, value);
        }
    }
}