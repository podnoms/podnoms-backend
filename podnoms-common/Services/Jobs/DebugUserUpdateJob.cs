using System;
using System.Threading.Tasks;
using EasyNetQ;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PodNoms.Common.Data.Messages;
using PodNoms.Common.Data.Settings;

namespace PodNoms.Common.Services.Jobs {
    public class DebugUserUpdateJob : IHostedJob {
        private readonly IBus _bus;
        private readonly AppSettings _settings;
        private readonly ILogger<DebugUserUpdateJob> _logger;

        public DebugUserUpdateJob(IOptions<AppSettings> settings, IBus bus, ILogger<DebugUserUpdateJob> logger) {
            _settings = settings.Value;
            this._logger = logger;
            this._bus = bus;
        }

        public async Task<bool> Execute() {
            return await Execute(null);
        }

        public async Task<bool> Execute(PerformContext context) {
            _logger.LogInformation($"Sending debug message to: {_settings.DebugUserId}");

            var message = new RealtimeUpdateMessage {
                UserId = _settings.DebugUserId,
                ChannelName = "site-notices",
                Title = "Debugging",
                Message = "This is a debug message",
                ImageUrl = "https://www.podnoms.com/assets/img/logo-icon.png"
            };
            await _bus.PubSub.PublishAsync(message);
            return true;
        }
    }
}
