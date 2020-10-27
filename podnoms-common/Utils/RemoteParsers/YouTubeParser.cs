using System;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Google.Apis.Requests;
using Microsoft.Extensions.Options;
using PodNoms.Common.Data.Settings;
using PodNoms.Common.Data.ViewModels.Remote.Google;
using Google.Apis.YouTube.v3;
using Google.Apis.Services;
using Nito.AsyncEx.Synchronous;
using PodNoms.Common.Persistence;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Data.Models;

namespace PodNoms.Common.Utils.RemoteParsers {
    public class ExternalServiceRequestLogger {
        private readonly IServicesApiKeyLoggerRepository _repository;
        private readonly IUnitOfWork _unitOfWork;

        public ExternalServiceRequestLogger(IServicesApiKeyLoggerRepository repository, IUnitOfWork unitOfWork) {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ServicesApiKeyLog> LogRequest(ServicesApiKey apiKey, string requesterId, string stackTrace) {
            var log = apiKey.LogRequest(requesterId, stackTrace);
            await _unitOfWork.CompleteAsync();
            return log;
        }
    }

    public class YouTubeParser : IYouTubeParser {
        const string URL_REGEX = @"^(http(s)?:\/\/)?((w){3}.)?youtu(be|.be)?(\.com)?\/.+";

        private readonly IHttpClientFactory _httpFactory;
        private readonly ExternalServiceRequestLogger _serviceRequestLogger;
        private readonly ILogger<YouTubeParser> _logger;
        private readonly ServicesApiKey _key;
        private readonly YouTubeService __client;

        public YouTubeParser(
            IOptions<AppSettings> options,
            ILogger<YouTubeParser> logger,
            IApiKeyRepository keyRepository,
            IHttpClientFactory httpFactory,
            ExternalServiceRequestLogger serviceRequestLogger) {
            _logger = logger;
            _key = keyRepository.GetApiKey("YouTube").Result;
            _httpFactory = httpFactory;
            _serviceRequestLogger = serviceRequestLogger;

            __client = new YouTubeService(new BaseClientService.Initializer {
                ApiKey = _key.Key,
                ApplicationName = GetType().ToString()
            });
        }

        public bool ValidateUrl(string url) {
            try {
                var regex = new Regex(URL_REGEX);
                var result = regex.Match(url);
                return result.Success;
            } catch (Exception) {
                return false;
            }
        }

        public async Task<string> ConvertUserToChannel(string url, string requesterId) {
            try {
                var channelName = string.Empty;
                var uriBuilder = new UriBuilder(url);
                if (url.Contains("/c/")) {
                    channelName = uriBuilder.Uri.Segments[2].TrimStart('/').TrimEnd('/');
                } else if (url.Contains("/user/")) {
                    channelName = uriBuilder.Uri.Segments[2].TrimStart('/').TrimEnd('/');
                }

                if (!string.IsNullOrEmpty(channelName)) {
                    using var client = _httpFactory.CreateClient("youtube");
                    var requestUrl =
                        $"channels?part=id&forUsername={channelName}&key={_key.Key}";

                    using var httpResponse = await client.GetAsync(requestUrl);
                    if (httpResponse.IsSuccessStatusCode) {
                        await using var stream = await httpResponse.Content.ReadAsStreamAsync();
                        var response = await JsonSerializer
                            .DeserializeAsync<YouTubeChannelApiQueryResult>(stream);

                        var c = response?.Items.SingleOrDefault();
                        if (c != null) {
                            return $"https://youtube.com/channel/{c.Id}";
                        }
                    }
                }
            } catch (Exception e) {
                _logger.LogError($"Error converting {url} to channel.", e);
            }

            return url;
        }

        public async Task<string> GetPlaylistId(string url, string requesterId) {
            return await Task.Run(() => {
                var query = System.Web.HttpUtility.ParseQueryString(new Uri(url).Query);
                return query["list"] ?? string.Empty;
            });
        }

        public async Task<string> GetVideoId(string url, string requesterId) {
            return await Task.Run(() => {
                if (!url.Contains("v=")) {
                    return string.Empty;
                }

                var videoId = url.Split("v=")[1];
                var ampersandPosition = videoId.IndexOf("&", StringComparison.Ordinal);
                if (ampersandPosition != -1) {
                    videoId = videoId.Substring(0, ampersandPosition);
                }

                return !string.IsNullOrEmpty(videoId) ? videoId : string.Empty;
            });
        }

        public async Task<List<ParsedItemResult>> GetEntries(string url, string requesterId, DateTime cutoffDate,
            int count = 10) {
            var channelType = await GetUrlType(url);
            var results = channelType switch {
                RemoteUrlType.Channel => await _getChannelItems(url, requesterId, DateTime.Now.AddDays(-28), count),
                RemoteUrlType.Playlist => await _getPlaylistItems(
                    await GetPlaylistId(url, requesterId),
                    requesterId,
                    DateTime.Now.AddDays(-28), count),
                _ => null
            };
            return results;
        }

        public async Task<RemoteVideoInfo> GetVideoInformation(string url, string requesterId) {
            var videoId = await GetVideoId(url, requesterId);
            if (string.IsNullOrEmpty(videoId)) {
                _logger.LogError($"Unable to get videoId for {url}");
            }

            var video = await _getVideoFromId(videoId, requesterId);
            if (!(video is null)) {
                return video;
            }

            _logger.LogError($"Unable to get info for video {url}");
            return null;
        }

        public async Task<RemoteUrlType> GetUrlType(string url) {
            return await Task.Run(() => {
                if (!ValidateUrl(url)) {
                    return RemoteUrlType.Invalid;
                }

                if (url.Contains("/channel/") || url.Contains("/c/") || url.Contains("/user/")) {
                    return RemoteUrlType.Channel;
                }

                return url.Contains("/playlist?") ? RemoteUrlType.Playlist : RemoteUrlType.SingleItem;
            });
        }

        #region Internals

        private YouTubeService _useClient(string requesterId) {
            _serviceRequestLogger
                .LogRequest(
                    _key,
                    requesterId,
                    Environment.StackTrace)
                .WaitAndUnwrapException();
            return __client;
        }

        private Task<T> _executeRequest<T>(ClientServiceRequest<T> request, string requesterId) {
            try {
                //TODO: request.ToString should be the API key in use
                return request.ExecuteAsync();
            } catch (Exception e) {
                //alert me
                _logger.LogError($"API Key Failure: {e.Message}");
                _logger.LogError($"API Key Failure: {request.Service.ApiKey}");
                _logger.LogError($"API Key Failure: {request.Service.ApplicationName}");
                throw;
            }
        }

        private async Task<RemoteVideoInfo> _getVideoFromId(string videoId, string requesterId) {
            var request = _useClient(requesterId).Videos.List("snippet");
            request.Id = videoId;
            request.MaxResults = 1;
            var response = await _executeRequest(request, requesterId);

            return response.Items
                .Select(video => new RemoteVideoInfo {
                    VideoId = video.Id.ToString(),
                    Title = video.Snippet.Title,
                    Description = video.Snippet.Description,
                    Thumbnail = video.Snippet.Thumbnails.High.Url,
                    Uploader = video.Snippet.ChannelTitle,
                    UploadDate = DateTime.Parse(video.Snippet.PublishedAt, null,
                        System.Globalization.DateTimeStyles.RoundtripKind)
                }).FirstOrDefault();
        }

        private async Task<string> _getChannelId(string url, string requesterId) {
            var search = _useClient(requesterId).Search.List("snippet");
            search.Type = "channel";
            search.Q = url;
            var response = await _executeRequest(search, requesterId);
            return response.Items.FirstOrDefault()?.Id.ChannelId;
        }

        private async Task<List<ParsedItemResult>> _getPlaylistItems(string playlistId, string requesterId,
            DateTime cutoff, int count) {
            if (string.IsNullOrEmpty(playlistId))
                return null;
            var query = _useClient(requesterId).PlaylistItems.List("snippet");
            query.PlaylistId = playlistId;
            query.MaxResults = count;

            var results = await _executeRequest(query, requesterId);
            var response = results
                .Items
                .Select(r => new ParsedItemResult {
                    Id = r.Snippet.ResourceId.VideoId,
                    Title = r.Snippet.Title,
                    VideoType = "YouTube",
                    UploadDate = DateTime.Parse(r.Snippet.PublishedAt, null,
                        System.Globalization.DateTimeStyles.RoundtripKind)
                })
                .OrderByDescending(r => r.UploadDate)
                .Take(count)
                .ToList();

            return response;
        }

        private async Task<List<ParsedItemResult>> _getChannelItems(string url, string requesterId, DateTime cutoff,
            int count) {
            var channelId = await _getChannelId(url, requesterId);
            if (string.IsNullOrEmpty(channelId)) {
                _logger.LogError($"Unable to get channel id for {url}");
            }

            //First we need to get the playlist id of the "uploads" playlist
            //Awkward but that's what this dude said https://www.youtube.com/watch?v=RjUlmco7v2M
            var query = _useClient(requesterId).Channels.List("contentDetails");
            query.Id = channelId;
            query.MaxResults = count;

            var results = await _executeRequest(query, requesterId);
            var uploadsPlaylistId = results.Items.FirstOrDefault()?
                .ContentDetails.RelatedPlaylists.Uploads;

            if (string.IsNullOrEmpty(uploadsPlaylistId)) {
                _logger.LogError($"Unable to find the uploads playlist for {url}");
            }

            var items = await _getPlaylistItems(uploadsPlaylistId, requesterId, cutoff, count);
            _logger.LogInformation($"Found {items.Count} items on channel {url}");
            return items;
        }

        #endregion
    }
}
