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

namespace PodNoms.Common.Services.NYT.Models
{
    #region Using

    #endregion

    /// <summary>
    ///     Class holding information about a video
    /// </summary>
    public class VideoDownloadInfo : DownloadInfo
    {
        public VideoDownloadInfo(VideoInfo info)
        {
            if (info is null)
            {
                Title = "Video deleted or otherwise unreachable";
                return;
            }

            Abr = info.abr;
            Acodec = info.acodec;
            AgeLimit = info.age_limit;
            AverageRating = info.average_rating;
            Categories = info.categories;
            Description = info.description;
            DislikeCount = info.dislike_count;
            DisplayId = info.display_id;
            Duration = info.duration;
            Ext = info.ext;
            Extractor = info.extractor;
            ExtractorKey = info.extractor_key;
            Format = info.format;
            FormatId = info.format_id;

            if (info.formats != null)
            {
                foreach (var formatInfo in info.formats)
                {
                    Formats.Add(new FormatDownloadInfo(formatInfo));
                }
            }

            Fps = info.fps;
            Height = info.height;
            Id = info.id;
            IeKey = info.ie_key;
            License = info.license;
            LikeCount = info.like_count;
            NEntries = info.n_entries;
            Playlist = info.playlist;
            PlaylistId = info.playlist_id;
            PlaylistIndex = info.playlist_index;
            PlaylistTitle = info.playlist_title;

            if (info.requested_formats != null)
            {
                foreach (var formatInfo in info.requested_formats)
                {
                    RequestedFormats.Add(new FormatDownloadInfo(formatInfo));
                }
            }

            Tags = info.tags;
            Thumbnail = info.thumbnail;

            if (info.thumbnails != null)
            {
                foreach (var thumbnail in info.thumbnails)
                {
                    Thumbnails.Add(new ThumbnailDownloadInfo(thumbnail));
                }
            }

            Title = info.title;
            UploadDate = info.upload_date;
            Uploader = info.uploader;
            UploaderId = info.uploader_id;
            UploaderUrl = info.uploader_url;
            Url = info.url;
            Vcodec = info.vcodec;
            ViewCount = info.view_count;
            WebpageUrl = info.webpage_url;
            WebpageUrlBasename = info.webpage_url_basename;
            Width = info.width;
        }

        public string Acodec { get; }

        public int? Abr { get; }

        public int? AgeLimit { get; }

        public double? AverageRating { get; }

        public List<string> Categories { get; }

        public string Description { get; }

        public int? DislikeCount { get; }

        public string DisplayId { get; }

        public int? Duration { get; }

        public string Ext { get; set; }

        public string Extractor { get; set; }

        public string ExtractorKey { get; set; }

        public string Format { get; }

        public string FormatId { get; }

        public List<FormatDownloadInfo> Formats { get; } = new List<FormatDownloadInfo>();

        public int? Fps { get; }

        public int? Height { get; }

        public string IeKey { get; }

        public string License { get; }

        public int? LikeCount { get; }

        public int? NEntries { get; }

        public string Playlist { get; }

        public string PlaylistId { get; }

        public int? PlaylistIndex { get; }

        public string PlaylistTitle { get; }

        public List<FormatDownloadInfo> RequestedFormats { get; } = new List<FormatDownloadInfo>();

        public string Thumbnail { get; }

        public string UploadDate { get; }

        public string Uploader { get; }

        public string UploaderId { get; }

        public string UploaderUrl { get; }

        public string Url { get; }

        public string Vcodec { get; }

        public int? ViewCount { get; }

        public string WebpageUrl { get; }

        public string WebpageUrlBasename { get; }

        public int? Width { get; }

        public List<ThumbnailDownloadInfo> Thumbnails { get; } = new List<ThumbnailDownloadInfo>();

        public List<string> Tags { get; }
    }
}