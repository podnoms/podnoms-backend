using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using NYoutubeDL;

namespace PodNoms.Common.Services.Jobs {
    public class UpdateYouTubeDlJob : IHostedJob {
        private readonly IMailSender _sender;
        private readonly ILogger _logger;

        public UpdateYouTubeDlJob(IMailSender sender, ILogger<ClientHeartbeatJob> logger) {
            _sender = sender;
            _logger = logger;
        }
        [AutomaticRetry(OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        public async Task<bool> Execute() { return await Execute(null); }

        [AutomaticRetry(OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        public async Task<bool> Execute(PerformContext context) {
            return await Task.Run(() => {
                _logger.LogInformation("Updating YoutubeDL");

                var yt = new YoutubeDL();
                yt.Options.GeneralOptions.Update = true;
                yt.Download("https://www.youtube.com/watch?v=OJ2wOKDzKyI");

                return true;
            });
        }
    }
}
