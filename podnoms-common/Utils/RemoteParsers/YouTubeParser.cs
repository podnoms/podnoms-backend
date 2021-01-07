using System;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Google;
using Google.Apis.Requests;
using Microsoft.Extensions.Options;
using PodNoms.Common.Data.Settings;
using PodNoms.Common.Data.ViewModels.Remote.Google;
using Google.Apis.YouTube.v3;
using Google.Apis.Services;
using Nito.AsyncEx.Synchronous;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Common.Utils.Extensions;
using PodNoms.Data.Models;
using Polly;
using Polly.Retry;

namespace PodNoms.Common.Utils.RemoteParsers {
    internal class ServiceWrapper {
        public ServiceWrapper(string requesterId, ServiceApiKey apiKey) {
            RequesterId = requesterId;
            ApiKey = apiKey;
            CreateOrRotateClient(apiKey);
        }

        public void CreateOrRotateClient(ServiceApiKey apiKey) {
            ApiKey = apiKey;
            Client = new YouTubeService(new BaseClientService.Initializer {
                ApplicationName = GetType().ToString(),
                ApiKey = apiKey.Key
            });
        }

        public string RequesterId { get; set; }
        public ServiceApiKey ApiKey { get; set; }
        public YouTubeService Client { get; set; }
    }

    public class YouTubeParser : IYouTubeParser {
        const string URL_REGEX = @"^(http(s)?:\/\/)?((w){3}.)?youtu(be|.be)?(\.com)?\/.+";

        private readonly IHttpClientFactory _httpFactory;
        private readonly ExternalServiceRequestLogger _serviceRequestLogger;
        private readonly ILogger<YouTubeParser> _logger;
        private readonly IApiKeyRepository _keyRepository;

        public YouTubeParser(
            IOptions<AppSettings> options,
            ILogger<YouTubeParser> logger,
            IApiKeyRepository keyRepository,
            IHttpClientFactory httpFactory,
            ExternalServiceRequestLogger serviceRequestLogger) {
            _logger = logger;
            _keyRepository = keyRepository;
            _httpFactory = httpFactory;
            _serviceRequestLogger = serviceRequestLogger;
        }

        #region Internals

        /// <summary>This method throws an exception.</summary>
        /// <param name="request">The pre-created Google Request object.</param>
        /// <param name="service">The PodNoms API call service wrapper to handle (and log) this exception.</param>
        /// <exception cref="ExpiredKeyException">
        /// This exception is thrown if the API key has expired
        /// </exception>
        private async Task<T> _executeWrappedRequest<T>(ClientServiceRequest<T> request,
            ServiceWrapper service) {
            try {
                var result = await request.ExecuteAsync();
                return result;
            } catch (GoogleApiException gae) {
                _logger.LogError($"API Key Failure: {gae.Message}");
                _logger.LogError($"API Key Failure: {service.RequesterId}");
                _logger.LogError($"API Key Failure: {service.ApiKey.Url}");
                _logger.LogError($"API Key Failure: {request.Service.ApiKey}");
                _logger.LogError($"API Key Failure: {request.Service.ApplicationName}");
                await _keyRepository.TaintKey(service.ApiKey, reason: gae.Message);

                throw new ExpiredKeyException(
                    $"Expired Key Exception\n" +
                    $"\tRequester: {service.RequesterId}" +
                    $"\tKey: {request.Service.ApiKey}" +
                    $"\tURL: {service.ApiKey.Url}"
                );
            }
        }

        /// <summary>This method throws an exception.</summary>
        /// <param name="request">The pre-created Google Request object.</param>
        /// <param name="service">The PodNoms API call service wrapper to handle (and log) this exception.</param>
        /// <exception cref="ExpiredKeyException">
        /// This exception is thrown if the API key has expired
        /// Can't currently find a way to change the service (and ApiKey)
        ///
        /// UPDATE - deprecated for fucking reasons.... keeping here as I still think I can get this to work
        /// </exception>
        private async Task<T> _____executeWrappedRequest<T>(ClientServiceRequest<T> request,
            ServiceWrapper service) {
            try {
                var policy = Policy
                    .Handle<GoogleApiException>()
                    .WaitAndRetryAsync(1,
                        (retryAttempt, exception, context) => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                        async (exception, timeSpan, retryCount, context) => {
                            _logger.LogError($"API Key Failure: {exception.Message}");
                            _logger.LogError($"API Key Failure: {service.RequesterId}");
                            _logger.LogError($"API Key Failure: {service.ApiKey.Url}");
                            _logger.LogError($"API Key Failure: {request.Service.ApiKey}");
                            _logger.LogError($"API Key Failure: {request.Service.ApplicationName}");

                            var newClient = await _useClient(service.RequesterId);
                            while (newClient.ApiKey.Equals(service.ApiKey)) {
                                newClient = await _useClient(service.RequesterId);
                            }

                            _logger.LogInformation(
                                $"Rotating keys...\n\t" +
                                $"Old key: {service.ApiKey.Key}\n\t" +
                                $"New Key: {newClient.ApiKey}");

                            var newRequest = request.Copy<ClientServiceRequest<T>>();

                            _logger.LogInformation($"Service API key changed to {newRequest.Service.ApiKey}");
                        });

                return await policy.ExecuteAsync(async () => {
                    if (request.Service.ApiKey.Equals("AIzaSyCroKXwyml2OEkWrizMyQmjgwXRtwdfHPA")) {
                        _logger.LogInformation("This is the expired key.");
                    }

                    var result = await request.ExecuteAsync();
                    return result;
                });
            } catch (Exception e) {
                _logger.LogError($"API Key Failure: {e.Message}");
                _logger.LogError($"API Key Failure: {service.RequesterId}");
                _logger.LogError($"API Key Failure: {service.ApiKey.Url}");
                _logger.LogError($"API Key Failure: {request.Service.ApiKey}");
                _logger.LogError($"API Key Failure: {request.Service.ApplicationName}");
                throw;
            }
        }

        private async Task<ServiceWrapper> _useClient(string requesterId) {
            var key = await _keyRepository.GetApiKey("YouTube", requesterId);
            if (key is null) {
                throw new NoKeyAvailableException();
            }   
            
            var client = new ServiceWrapper(requesterId, key);
            await _serviceRequestLogger
                .LogRequest(
                    key,
                    requesterId,
                    Environment.StackTrace);
            return client;
        }

        private async Task<RemoteVideoInfo> _getVideoFromId(string videoId, string requesterId) {
            var client = (await _useClient(requesterId));
            var request = client.Client.Videos.List("snippet");
            request.Id = videoId;
            request.MaxResults = 1;
            var response = await _executeWrappedRequest(request, client);
            return response.Items
                .Select(video => new RemoteVideoInfo {
                    VideoId = video.Id.ToString(),
                    Title = video.Snippet.Title,
                    Description = video.Snippet.Description,
                    Thumbnail = video.Snippet.Thumbnails.High.Url,
                    Uploader = video.Snippet.ChannelTitle,
                    UploadDate = DateTime.Parse(
                        video.Snippet.PublishedAt,
                        null,
                        System.Globalization.DateTimeStyles.RoundtripKind)
                }).FirstOrDefault();
        }

        private async Task<string> _getChannelId(string url, string requesterId) {
            var client = (await _useClient(requesterId));
            var search = client.Client.Search.List("snippet");
            search.Type = "channel";
            search.Q = url;
            var response = await _executeWrappedRequest(search, client);
            return response.Items.FirstOrDefault()?.Id.ChannelId;
        }

        private async Task<List<ParsedItemResult>> _getPlaylistItems(string playlistId, string requesterId,
            DateTime cutoff, int count) {
            if (string.IsNullOrEmpty(playlistId))
                return null;
            var client = (await _useClient(requesterId));
            var query = client.Client.PlaylistItems.List("snippet");
            query.PlaylistId = playlistId;
            query.MaxResults = count;

            var results = await _executeWrappedRequest(query, client);
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
            var client = (await _useClient(requesterId));
            var query = client.Client.Channels.List("contentDetails");
            query.Id = channelId;
            query.MaxResults = count;

            var results = await _executeWrappedRequest(query, client);
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
                    var key = await _keyRepository.GetApiKey("YouTube", requesterId);
                    var requestUrl =
                        $"channels?part=id&forUsername={channelName}&key={key.Key}";

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
                return null;
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
    }
}
