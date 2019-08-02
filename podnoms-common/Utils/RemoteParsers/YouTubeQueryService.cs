using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YoutubeExplode;

namespace PodNoms.Common.Utils.RemoteParsers {
    using System;
    using System.Net.Http;
    using Microsoft.Extensions.Logging;
    using PodNoms.Common.Services.NYT.Models;

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

        public async Task<string> GetChannelId(string channelName) {
            return await _client.GetChannelIdAsync(channelName);
        }
        public string GetChannelIdentifier(string url) {
            var type = GetUrlType(url);
            if (type == RemoteUrlType.Channel) {
                YoutubeClient.TryParseChannelId(url, out var channelId);
                return channelId;
            }
            if (type == RemoteUrlType.Playlist) {
                YoutubeClient.TryParsePlaylistId(url, out var playlistId);
                return playlistId;
            }
            if (type == RemoteUrlType.User) {
                YoutubeClient.TryParseUsername(url, out var userId);
                return userId;
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
                    case RemoteUrlType.User:
                        results = await _getUserItems(url, count);
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
            if (YoutubeClient.TryParsePlaylistId(url, out var playlistId)) {
                var playlist = await _client.GetPlaylistAsync(playlistId, 1);
                //Can't get date added to playlist
                //but items appear to be returned in order 
                var results = playlist.Videos
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
            throw new YoutubeChannelParseException($"Unable to parse channel id from channel {url}");
        }
        private async Task<List<ParsedItemResult>> _getChannelItems(string url, int count = 10) {
            if (YoutubeClient.TryParseChannelId(url, out var channelId)) {
                return await _getItemsFromId(channelId);
            }
            throw new YoutubeChannelParseException($"Unable to parse channel id from channel {url}");
        }
        private async Task<List<ParsedItemResult>> _getUserItems(string url, int count = 10) {
            if (YoutubeClient.TryParseUsername(url, out var userName)) {
                var channelId = await GetChannelId(userName);
                if (!string.IsNullOrEmpty(channelId)) {
                    return await _getItemsFromId(channelId);
                }
            }
            throw new YoutubeChannelParseException($"Unable to parse channel id from user {url}");
        }

        private async Task<List<ParsedItemResult>> _getItemsFromId(string channelId, int count = 10) {

            var videos = await _client.GetChannelUploadsAsync(channelId, 1);
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
                if (!YoutubeClient.TryParseVideoId(url, out videoId)) {
                    return null;
                }
            }
            if (!string.IsNullOrEmpty(videoId)) {
                try{
                    var info = await _client.GetVideoAsync(videoId);
                    if (info != null) {
                        return new RemoteVideoInfo {
                            VideoId = info.Id,
                            Title = info.Title,
                            Description = info.Description,
                            Thumbnail = info.Thumbnails.StandardResUrl,
                            Uploader = info.Author,
                            UploadDate = info.UploadDate.Date
                        };
                    }
                }catch(Exception ex){
                    _logger.LogError($"Error parsing video {url}");
                    _logger.LogError(ex.Message);
                }
            }
            return null;
        }

        public RemoteUrlType GetUrlType(string url) {

            // Video ID
            if (YoutubeClient.ValidateVideoId(url)) {
                return RemoteUrlType.SingleItem;
            }

            // Video URL
            if (YoutubeClient.TryParseVideoId(url, out var videoId)) {
                return RemoteUrlType.SingleItem;
            }

            // Playlist ID
            if (YoutubeClient.ValidatePlaylistId(url)) {
                return RemoteUrlType.Playlist;
            }

            // Playlist URL
            if (YoutubeClient.TryParsePlaylistId(url, out var playlistId)) {
                return RemoteUrlType.Playlist;
            }

            // Channel ID
            if (YoutubeClient.ValidateChannelId(url)) {
                return RemoteUrlType.Channel;
            }

            // Channel URL
            if (YoutubeClient.TryParseChannelId(url, out var channelId)) {
                return RemoteUrlType.Channel;
            }

            // User URL
            if (YoutubeClient.TryParseUsername(url, out var username)) {
                return RemoteUrlType.User;
            }

            // Search
            {
                return RemoteUrlType.Invalid;
            }
        }
    }
}
