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
using PodNoms.Common.Persistence.Repositories;
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
using Tweetinvi.Streaming;

namespace PodNoms.Common.Services.Social {
    public class EpisodeFromTweetHandler : ITweetListener {
        private readonly TwitterStreamListenerSettings _twitterSettings;
        private readonly AppSettings _appSettings;
        private readonly JwtIssuerOptions _jwtOptions;
        private readonly ILogger<EpisodeFromTweetHandler> _logger;
        private readonly IServiceProvider _provider;
        private readonly IHttpClientFactory _clientFactory;

        private IFilteredStream? _stream;

        public EpisodeFromTweetHandler(
            IOptions<TwitterStreamListenerSettings> twitterSettings,
            IOptions<AppSettings> appSettings,
            IOptions<JwtIssuerOptions> jwtOptions,
            ILogger<EpisodeFromTweetHandler> logger,
            IServiceProvider provider,
            IHttpClientFactory clientFactory
        ) {
            _twitterSettings = twitterSettings.Value;
            _appSettings = appSettings.Value;
            _jwtOptions = jwtOptions.Value;
            _logger = logger;
            _provider = provider;
            _clientFactory = clientFactory;
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

        private async Task<bool> _sendReply(Tweetinvi.Models.ITweet tweet, string message) {
            var result = await TweetAsync.PublishTweetInReplyTo(message, tweet.Id);
            return result != null;
        }

        private async void __tryCreateEpisode(object? sender, MatchedTweetReceivedEventArgs incomingTweet) {
            _logger.LogDebug(incomingTweet.Json);

            ExceptionHandler.SwallowWebExceptions = false;
            ExceptionHandler.LogExceptions = true;
            try {
                if (incomingTweet.Tweet.InReplyToStatusId == null) {
                    return;
                }

                var tweetId = (long)incomingTweet.Tweet.InReplyToStatusId;
                var sourceTweet = await TweetAsync.GetTweet((long)incomingTweet.Tweet.InReplyToStatusId);
                var tweetToReplyTo = incomingTweet.Tweet;
                var targetUser = incomingTweet.Tweet.CreatedBy.ScreenName;

                var user = await __getTargetUser(targetUser);
                if (user == null) {
                    await _createPublicErrorResponse(
                        tweetToReplyTo,
                        $"Hi @{targetUser}, sorry but I cannot find your account.\nPlease edit your profile and make sure your Twitter Handle is set correctly.\n{_appSettings.SiteUrl}/profile"
                    );
                    return;
                }

                using var scope = _provider.CreateScope();
                var (
                    processor,
                    podcastRepository,
                    entryRepository,
                    unitOfWork) = _getScopedServices(scope);

                // var podcast = (await podcastRepository.GetRandomForUser(user.Id));
                var podcast = await __getTargetPodcast(tweetToReplyTo.FullText, user.Id, podcastRepository);
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

                var status = await processor.GetInformation(entry);
                if (status != RemoteUrlType.SingleItem) {
                    await _createPublicErrorResponse(
                        tweetToReplyTo,
                        $"Hi @{targetUser}, sorry but I cannot find any media to parse in this tweet.\n{_appSettings.SiteUrl}"
                    );
                    return;
                }

                entry.Title = $"New entry from {sourceTweet.CreatedBy.ScreenName}'s tweet";
                entry.Description = sourceTweet.Text;

                entryRepository.AddOrUpdate(entry);
                await unitOfWork.CompleteAsync();
                await _sendHubUpdate(user.Id.ToString(), entry.SerialiseForHub());
                //get JWT token so we can call into the job realtime stuff
                var token = await __getJwtTokenForUser(user);
                var TEMPTOKENDONOTUSE =
                    "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJmZXJnYWwubW9yYW4rcG9kbm9tc3Rlc3RAZ21haWwuY29tIiwianRpIjoiMjk0NGU5MTYtOGNlMy00NzkxLThiMmYtOWI2NWU3Y2VmN2QwIiwiaWF0IjoxNTg4ODk3OTIxLCJyb2wiOiJhcGlfYWNjZXNzIiwiaWQiOiIyMzA1MjRjMi02MWIwLTRjNWItYTg1OS1mN2Y3NWMyNGIzNzIiLCJuYmYiOjE1ODg4OTc5MjEsImV4cCI6MTU5MTQwMzUyMSwiaXNzIjoicG9kbm9tc0FwaSIsImF1ZCI6Imh0dHA6Ly9wb2Rub21zLmxvY2FsOjUwMDAvIn0.wugkrWa2uPk08eTFrTizPenLuPAMj4WFbtT2GDztGVU";
                var processId = BackgroundJob.Enqueue<ProcessNewEntryJob>(
                    e => e.ProcessEntry(entry.Id, token, null));

                var message =
                    $"Hi @{targetUser}, your request was processed succesfully, you can find your new episode in your podcatcher or here\n{podcast.GetPagesUrl(_appSettings.PagesUrl)}";
                BackgroundJob.ContinueJobWith<SendTweetJob>(
                    processId, (r) => r.SendTweet(tweetToReplyTo.Id, message));
            } catch (Exception e) {
                _logger.LogError($"Error creating episode: {e.Message}");
            }
        }

        private async Task _sendHubUpdate(string userId, RealtimeEntityUpdateMessage message) {
            using var client = _clientFactory.CreateClient("podnoms");
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

        private (IUrlProcessService, IPodcastRepository, IEntryRepository, IUnitOfWork unitOfWork) _getScopedServices(
            IServiceScope scope) {
            var processService = scope.ServiceProvider.GetRequiredService<IUrlProcessService>();
            var podcastRepository = scope.ServiceProvider.GetRequiredService<IPodcastRepository>();
            var entryRepository = scope.ServiceProvider.GetRequiredService<IEntryRepository>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            return (
                processService,
                podcastRepository,
                entryRepository,
                unitOfWork
            );
        }

        private async Task<ApplicationUser> __getTargetUser(string twitterHandle) {
            using var scope = _provider.CreateScope();
            _logger.LogDebug($"Finding user");
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await userManager.FindByTwitterHandleAsync(twitterHandle);
            return user;
        }

        private async Task<Podcast?> __getTargetPodcast(string twitterText, string userId,
            IPodcastRepository podcastRepository) {

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

            var podcast = await podcastRepository.GetForUserAndSlugAsync(Guid.Parse(userId), podcastSlug);
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
                roles.ToArray<string>(),
                _jwtOptions,
                new JsonSerializerSettings {Formatting = Formatting.Indented});
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
                Tweetinvi.Auth.SetUserCredentials(
                    _twitterSettings.ApiKey,
                    _twitterSettings.ApiKeySecret,
                    _twitterSettings.AccessToken,
                    _twitterSettings.AccessTokenSecret);
                _stream = Stream.CreateFilteredStream();

                _logger.LogInformation("Starting Twitter stream");


                _stream.AddTrack(_twitterSettings.Track);
                _stream.MatchingTweetReceived += __tryCreateEpisode;

                await _stream.StartStreamMatchingAnyConditionAsync();
            } catch (TwitterNullCredentialsException) {
                _logger.LogError("Twitter settings are incorrect or missing, stopping listener");
            } catch (Exception e) {
                _logger.LogError($"Unknown error starting stream: {e.Message}");
            }
        }

        private Task __stopStreamInternal() {
            _logger.LogInformation("Stopping Twitter stream");
            return Task.Factory.StartNew(() => {
                _stream?.StopStream();
            });
        }
    }
}
