using System;
using System.Threading.Tasks;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PodNoms.Common.Data.Settings;
using Tweetinvi;
using Tweetinvi.Parameters;

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

        public async Task<bool> SendTweet(long tweetId, string message) {
            var client = new TwitterClient(
                _twitterSettings.ApiKey,
                _twitterSettings.ApiKeySecret,
                _twitterSettings.AccessToken,
                _twitterSettings.AccessTokenSecret);
            var result = await client.Tweets.PublishTweetAsync(new PublishTweetParameters {
                Text = message,
                InReplyToTweetId = tweetId
            });
            return result is not null;
        }
    }
}
