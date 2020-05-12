using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using PodNoms.Common.Services.Social;

namespace PodNoms.Jobs.Services {
    public class TweetListenerService : BackgroundService {
        private readonly ITweetListener _tweetListener;
        public TweetListenerService(ITweetListener tweetListener) {
            _tweetListener = tweetListener;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            await _tweetListener.StartAsync();
        }
        public override async Task StopAsync(CancellationToken cancellationToken) {
            await _tweetListener.StopAsync();
        }
    }
}
