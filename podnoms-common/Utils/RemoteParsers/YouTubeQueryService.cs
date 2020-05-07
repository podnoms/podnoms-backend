using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YoutubeExplode;
using System;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using YoutubeExplode.Channels;
using YoutubeExplode.Playlists;
using YoutubeExplode.Videos;

namespace PodNoms.Common.Utils.RemoteParsers {
    public class YouTubeQueryService : IYouTubeParser {
        const string URL_REGEX = @"^(http(s)?:\/\/)?((w){3}.)?youtu(be|.be)?(\.com)?\/.+";

        private readonly YoutubeClient _client = new YoutubeClient();
        private readonly ILogger<YouTubeQueryService> _logger;

        public YouTubeQueryService(ILogger<YouTubeQueryService> logger) {
            this._logger = logger;
        }

        public bool ValidateUrl(string url) {
            var regex = new Regex(URL_REGEX);
            var result = regex.Match(url);
            return result.Success;
        }

        public async Task<string> GetVideoId(string url) {
            var video = await _client.Videos.GetAsync(url);
            return video.Id.Value;
        }

        public async Task<string> GetChannelId(string channelName) {
            var channel = await _client.Channels.GetAsync(channelName);
            return channel.Id.Value;
        }

        public async Task<string> GetChannelIdentifier(string url) {
            var type = GetUrlType(url);
            if (type == RemoteUrlType.Channel) {
                var channel = await _client.Channels.GetAsync(url);
                return channel.Id.Value;
            }

            if (type == RemoteUrlType.Playlist) {
                var playlist = await _client.Playlists.GetAsync(url);
                return playlist.Id.Value;
            }

            return string.Empty;
        }

        public async Task<List<ParsedItemResult>> GetPlaylistItems(string url, DateTime cutoffDate, int count = 10) {
            //need to do a little dance here.. URL can be
            //  1. User
            //  2. Channel
            //  3. Playlist
            //all of which can translate to a PodNoms playlist
            try {
                var channelType = GetUrlType(url);
                List<ParsedItemResult> results = null;
                switch (channelType) {
                    case RemoteUrlType.Channel:
                        results = await _getChannelItems(url, count);
                        break;
                    case RemoteUrlType.Playlist:
                        results = await _getPlaylistItems(url, count);
                        break;
                }

                if (results != null) {
                    return results
                        // .Where(r => r.UploadDate >= cutoffDate)
                        .ToList();
                }
            } catch (HttpRequestException e) {
                if (e.Message.Contains("400 (Bad Request)")) {
                    throw new PlaylistExpiredException(e.Message);
                }
            }

            throw new YoutubeChannelParseException($"Unknown channel type {url}");
        }

        private async Task<List<ParsedItemResult>> _getPlaylistItems(string url, int count = 10) {
            var videos = await _client.Playlists.GetVideosAsync(url);
            var results = videos
                .Select(r => new ParsedItemResult {
                    Id = r.Id,
                    Title = r.Title,
                    VideoType = "YouTube",
                    UploadDate = r.UploadDate.Date
                })
                .Reverse()
                .Take(count).ToList();
            return results;
        }

        private async Task<List<ParsedItemResult>> _getChannelItems(string url, int count = 10) {
            var videos = await _client.Channels.GetUploadsAsync(url);
            return videos
                .Select(r => new ParsedItemResult {
                    Id = r.Id,
                    Title = r.Title,
                    VideoType = "YouTube",
                    UploadDate = r.UploadDate.Date
                })
                .OrderByDescending(r => r.UploadDate)
                .Take(count).ToList();
        }

        public async Task<RemoteVideoInfo> GetInformation(string url) {
            var videoId = url;
            if (url.StartsWith("http")) {
                videoId = await GetVideoId(url);
                if (string.IsNullOrEmpty(videoId)) {
                    return null;
                }
            }

            if (!string.IsNullOrEmpty(videoId)) {
                try {
                    var info = await _client.Videos.GetAsync(new VideoId(videoId));
                    return new RemoteVideoInfo {
                        VideoId = info.Id,
                        Title = info.Title,
                        Description = info.Description,
                        Thumbnail = info.Thumbnails.HighResUrl,
                        Uploader = info.Author,
                        UploadDate = info.UploadDate.Date
                    };
                } catch (Exception ex) {
                    _logger.LogError($"Error parsing video {url}");
                    _logger.LogError(ex.Message);
                }
            }
            return null;
        }

        public RemoteUrlType GetUrlType(string url) {
            // Video ID
            try {
                new VideoId(url);
                return RemoteUrlType.SingleItem;
            } catch (ArgumentException) { }

            // Playlist ID
            try {
                new PlaylistId(url);
                return RemoteUrlType.Playlist;
            } catch (ArgumentException) { }

            // Channel ID
            try {
                new ChannelId(url);
                return RemoteUrlType.Channel;
            } catch (ArgumentException) { }

            {
                return RemoteUrlType.Invalid;
            }
        }
    }
}
