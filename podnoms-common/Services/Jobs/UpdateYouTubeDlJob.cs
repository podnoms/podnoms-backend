using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PodNoms.Common.Services.NYT;

namespace PodNoms.Common.Services.Jobs {
    public class UpdateYouTubeDlJob : IJob {
        private readonly IMailSender _sender;
        private readonly ILogger _logger;

        public UpdateYouTubeDlJob(IMailSender sender, ILogger<UpdateYouTubeDlJob> logger) {
            _sender = sender;
            _logger = logger;
        }

        public async Task<bool> Execute() {
            _logger.LogInformation("Updating YoutubeDL");

            var yt = new YoutubeDL();
            yt.Options.GeneralOptions.Update = true;
            yt.Download("https://www.youtube.com/watch?v=OJ2wOKDzKyI");

            return true;
        }
    }
}
