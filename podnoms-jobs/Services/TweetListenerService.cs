using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using PodNoms.Common.Services.Social;

namespace PodNoms.Jobs.Services {
    public class TweetListenerService : IHostedService {
        private readonly ITweetListener _tweetListener;

        public TweetListenerService(ITweetListener tweetListener) {
            _tweetListener = tweetListener;
        }
        public async Task StartAsync(CancellationToken cancellationToken) {
            await _tweetListener.StartAsync();
        }

        public async Task StopAsync(CancellationToken cancellationToken) {
            await _tweetListener.StopAsync();
        }
    }
}
