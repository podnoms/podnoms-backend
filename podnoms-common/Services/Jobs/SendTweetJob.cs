using System;
using System.Threading.Tasks;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PodNoms.Common.Data.Settings;
using Tweetinvi;
using Tweetinvi.Models;

namespace PodNoms.Common.Services.Jobs {
    public class SendTweetJob : AbstractHostedJob {
        private readonly TwitterStreamListenerSettings _twitterSettings;

        public SendTweetJob(ILogger<SendTweetJob> logger,
                    IOptions<TwitterStreamListenerSettings> twitterSettings) : base(logger) {
            _twitterSettings = twitterSettings.Value;
        }
        public override Task<bool> Execute(PerformContext context) {
            throw new NotImplementedException();
        }

        public async Task SendTweet(long tweetId, string message) {
            Tweetinvi.Auth.SetUserCredentials(
                _twitterSettings.ApiKey,
                _twitterSettings.ApiKeySecret,
                _twitterSettings.AccessToken,
                _twitterSettings.AccessTokenSecret);
            var result = await TweetAsync.PublishTweetInReplyTo(message, tweetId);
        }
    }
}
