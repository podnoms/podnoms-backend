#nullable enable
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PodNoms.Common.Data.Settings;
using PodNoms.Common.Persistence.Repositories;
using Tweetinvi;
using Tweetinvi.Events;
using Tweetinvi.Exceptions;
using Tweetinvi.Streaming;

namespace PodNoms.Common.Services.Social {

    public class EpisodeFromTweetHandler : ITweetListener {
        private readonly TwitterStreamListenerSettings _twitterSettings;
        private readonly IFilteredStream _stream;
        private readonly ILogger<EpisodeFromTweetHandler> _logger;
        private readonly IPodcastRepository _podcastRepository;
        private readonly IEntryRepository _entryRepository;

        public EpisodeFromTweetHandler(
                    IOptions<TwitterStreamListenerSettings> twitterSettings,
                    ILogger<EpisodeFromTweetHandler> logger,
                    IPodcastRepository podcastRepository,
                    IEntryRepository entryRepository) {
            _twitterSettings = twitterSettings.Value;
            _logger = logger;
            _podcastRepository = podcastRepository;
            _entryRepository = entryRepository;
            Tweetinvi.Auth.SetUserCredentials(
                _twitterSettings.ApiKey,
                _twitterSettings.ApiKeySecret,
                _twitterSettings.AccessToken,
                _twitterSettings.AccessTokenSecret);
            _stream = Stream.CreateFilteredStream();
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
        private async void __tryCreateEpisode(object? sender, MatchedTweetReceivedEventArgs incomingTweet) {
            _logger.LogInformation($"Matching tweet received: {incomingTweet.Tweet}");
            _logger.LogDebug(incomingTweet.Json);

            ExceptionHandler.SwallowWebExceptions = false;
            ExceptionHandler.LogExceptions = true;
            try {
                if (incomingTweet.Tweet.InReplyToStatusId == null) {
                    return;
                }

                var tweetId = (long)incomingTweet.Tweet.InReplyToStatusId;
                var tweetToReplyTo = await TweetAsync.GetTweet(tweetId);

                var textToPublish = $"@{tweetToReplyTo.CreatedBy.ScreenName} Message Received!";
                var tweet = await TweetAsync.PublishTweetInReplyTo(textToPublish, tweetId);

                _logger.LogInformation("Publish success? {0}", tweet != null);
            } catch (Exception e) {
                _logger.LogError($"Error creating episode: {e.Message}");
            }
        }

        public async Task __startStreamInternal() {
            try {
                _logger.LogInformation("Starting Twitter stream");

                _stream.AddTrack("@podnoms");
                _stream.MatchingTweetReceived += __tryCreateEpisode;

                await _stream.StartStreamMatchingAllConditionsAsync();

            } catch (TwitterNullCredentialsException e) {
                _logger.LogError("Twitter settings are incorrect or missing, stopping listener");
            } catch (Exception e) {
                _logger.LogError($"Unknown error starting stream: {e.Message}");
            }
        }

        public Task __stopStreamInternal() {
            _logger.LogInformation("Stopping Twitter stream");
            return Task.Factory.StartNew(() => {
                _stream.StopStream();
            });
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
    }
}
