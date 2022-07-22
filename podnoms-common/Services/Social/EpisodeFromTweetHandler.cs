#nullable enable
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using PodNoms.Common.Auth;
using PodNoms.Common.Data.Settings;
using PodNoms.Common.Persistence;
using PodNoms.Common.Services.Jobs;
using PodNoms.Common.Services.Processor;
using PodNoms.Common.Utils.Extensions;
using PodNoms.Common.Utils.RemoteParsers;
using PodNoms.Data.Enums;
using PodNoms.Data.Models;
using PodNoms.Data.ViewModels;
using Tweetinvi;
using Tweetinvi.Events;
using Tweetinvi.Exceptions;
using Tweetinvi.Models;
using Tweetinvi.Parameters;
using Tweetinvi.Streaming;

namespace PodNoms.Common.Services.Social {
    public class EpisodeFromTweetHandler : ITweetListener {
        private readonly TwitterStreamListenerSettings _twitterSettings;
        private readonly AppSettings _appSettings;
        private readonly ILogger<EpisodeFromTweetHandler> _logger;
        private readonly IServiceProvider _provider;
        private readonly IHttpClientFactory _httpClientFactory;

        private IFilteredStream? _stream;
        private IUrlProcessService _processor;
        private IRepoAccessor _repo;
        private readonly TwitterClient _twitterClient;

        public EpisodeFromTweetHandler(
            IOptions<TwitterStreamListenerSettings> twitterSettings,
            IOptions<AppSettings> appSettings,
            IOptions<JwtIssuerOptions> jwtOptions,
            ILogger<EpisodeFromTweetHandler> logger,
            IServiceProvider provider,
            IHttpClientFactory httpClientFactory
        ) {
            _twitterSettings = twitterSettings.Value;
            _appSettings = appSettings.Value;
            _logger = logger;
            _provider = provider;
            _httpClientFactory = httpClientFactory;
            _twitterClient = new TwitterClient(
                _twitterSettings.ApiKey,
                _twitterSettings.ApiKeySecret,
                _twitterSettings.AccessToken,
                _twitterSettings.AccessTokenSecret
            );

            (_processor, _repo) = __getScopedServices(_provider.CreateScope());
        }

        public async Task<bool> StartAsync() {
            if (!__checkSettings()) {
                _logger.LogError("Twitter settings are incorrect or missing, stopping listener");
                return false;
            }

            await __startStreamInternal();

            return true;
        }

        public async Task StopAsync() {
            await __stopStreamInternal();
        }

        private bool __checkSettings() {
            return !string.IsNullOrEmpty(_twitterSettings.AccessToken) &&
                   !string.IsNullOrEmpty(_twitterSettings.AccessTokenSecret) &&
                   !string.IsNullOrEmpty(_twitterSettings.ApiKey) &&
                   !string.IsNullOrEmpty(_twitterSettings.ApiKeySecret);
        }

        private void __handleError(Exception e) {
            _logger.LogError($"Error consuming tweet: {e.Message}");
        }

        private async Task<bool> _sendReply(ITweet tweet, string message) {
            var result = await _twitterClient.Tweets.PublishTweetAsync(new PublishTweetParameters {
                Text = message,
                InReplyToTweetId = tweet.Id
            });
            return result != null;
        }

        private async void __tryCreateEpisode(object? sender, MatchedTweetReceivedEventArgs incomingTweet) {
            try {
                if (incomingTweet.Tweet.InReplyToStatusId == null) {
                    return;
                }

                var tweetId = (long)incomingTweet.Tweet.InReplyToStatusId;
                var sourceTweet = await _twitterClient.Tweets.GetTweetAsync(tweetId);
                var tweetToReplyTo = incomingTweet.Tweet;
                var targetUser = incomingTweet.Tweet.CreatedBy.ScreenName;

                var user = await __getTargetUser(targetUser);

                // var podcast = (await podcastRepository.GetRandomForUser(user.Id));
                var podcast = await __getTargetPodcast(tweetToReplyTo.FullText, user.Id);
                if (podcast == null) {
                    await _createPublicErrorResponse(
                        tweetToReplyTo,
                        $"Hi @{targetUser}, I cannot find the podcast to create this episode for, please make sure the podcast slug or URL is the first word after {_twitterSettings.Track}\n{_appSettings.SiteUrl}"
                    );
                    return;
                }

                var entry = new PodcastEntry {
                    Podcast = podcast,
                    Processed = false,
                    ProcessingStatus = ProcessingStatus.Accepted,
                    SourceUrl = sourceTweet.Url
                };

                var status = await _processor.GetInformation(entry, podcast.AppUserId);
                if (status != RemoteUrlType.SingleItem) {
                    await _createPublicErrorResponse(
                        tweetToReplyTo,
                        $"Hi @{targetUser}, sorry but I cannot find any media to parse in this tweet.\n{_appSettings.SiteUrl}"
                    );
                    return;
                }

                entry.Title = $"New entry from {sourceTweet.CreatedBy.ScreenName}'s tweet";
                entry.Description = sourceTweet.Text;

                _repo.Entries.AddOrUpdate(entry);
                await _repo.CompleteAsync();
                await _sendHubUpdate(user.Id.ToString(), entry.SerialiseForHub());
                //get JWT token so we can call into the job realtime stuff
                var token = await __getJwtTokenForUser(user);
                var processId = BackgroundJob.Enqueue<ProcessNewEntryJob>(
                    e => e.ProcessEntry(entry.Id, null));

                var message =
                    $"Hi @{targetUser}, your request was processed succesfully, you can find your new episode in your podcatcher or here\n{podcast.GetPagesUrl(_appSettings.PagesUrl)}";
                BackgroundJob.ContinueJobWith<SendTweetJob>(
                    processId, (r) => r.SendTweet(tweetToReplyTo.Id, message));
            } catch (Exception e) {
                _logger.LogError($"Error creating episode: {e.Message}");
            }
        }

        private async Task _sendHubUpdate(string userId, RealtimeEntityUpdateMessage message) {
            using var client = _httpClientFactory.CreateClient("podnoms");
            var payload = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(message),
                Encoding.UTF8,
                "application/json");
            var response = await client.PostAsync(
                $"realtime/update/{userId}",
                payload);
            if (!response.IsSuccessStatusCode) {
                _logger.LogError(
                    $"Error updating realtime hub.\n\tReason: {response.ReasonPhrase}\n\t{response.Content}");
            }
        }

        private (IUrlProcessService, IRepoAccessor repo) __getScopedServices(
            IServiceScope scope) {
            var processService = scope.ServiceProvider.GetRequiredService<IUrlProcessService>();
            var repo = scope.ServiceProvider.GetRequiredService<IRepoAccessor>();
            return (
                processService,
                repo
            );
        }

        private async Task<ApplicationUser> __getTargetUser(string twitterHandle) {
            using var scope = _provider.CreateScope();
            _logger.LogDebug($"Finding user");
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await userManager.FindByTwitterHandleAsync(twitterHandle);
            return user;
        }

        private async Task<Podcast?> __getTargetPodcast(string twitterText, string userId) {
            _logger.LogDebug($"Finding podcast for tweet");
            var podcastSlug = twitterText
                .FindStringFollowing(_twitterSettings.Track)
                .TrimEnd('/');
            if (string.IsNullOrEmpty(podcastSlug)) {
                return null;
            }

            if (podcastSlug.Contains("/")) {
                podcastSlug = podcastSlug.Split('/').Last();
            }

            var podcast = await _repo.Podcasts.GetForUserAndSlugAsync(Guid.Parse(userId), podcastSlug);
            return podcast;
        }

        private async Task<string> __getJwtTokenForUser(ApplicationUser user) {
            using var scope = _provider.CreateScope();
            _logger.LogDebug($"Finding user");
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var jwtFactory = scope.ServiceProvider.GetRequiredService<IJwtFactory>();
            var roles = await userManager.GetRolesAsync(user);
            var jwt = await TokenIssuer.GenerateRawJwt(
                jwtFactory.GenerateClaimsIdentity(user.UserName, user.Id),
                jwtFactory,
                user.UserName,
                roles.ToArray<string>());
            return jwt;
        }

        private async Task _createPublicErrorResponse(ITweet tweetToReplyTo, string text) {
            _logger.LogError($"Error parsing incoming tweet.\n\t{text}");
            await _sendReply(
                tweetToReplyTo,
                text);
            return;
        }

        private async Task __startStreamInternal() {
            try {
                _stream = _twitterClient.Streams.CreateFilteredStream();
                _logger.LogInformation("Starting Twitter stream");

                _stream.AddTrack(_twitterSettings.Track);
                _stream.MatchingTweetReceived += __tryCreateEpisode;

                await _stream.StartMatchingAnyConditionAsync();
            } catch (TwitterNullCredentialsException) {
                _logger.LogError("Twitter settings are incorrect or missing, stopping listener");
            } catch (Exception e) {
                _logger.LogError($"Unknown error starting stream: {e.Message}");
            }
        }

        private Task __stopStreamInternal() {
            _logger.LogInformation("Stopping Twitter stream");
            return Task.Factory.StartNew(() => {
                _stream?.Stop();
            });
        }
    }
}
