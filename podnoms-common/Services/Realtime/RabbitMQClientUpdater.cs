using System;
using System.Threading.Tasks;
using EasyNetQ;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using PodNoms.Common.Data.Messages;
using PodNoms.Common.Services.Hubs;

namespace PodNoms.Common.Services.Realtime {
    public class RabbitMQClientUpdater : IRealTimeUpdater {
        private readonly IBus _bus;
        private readonly ILogger<RabbitMQClientUpdater> _logger;

        public RabbitMQClientUpdater(IBus bus, ILogger<RabbitMQClientUpdater> logger) {
            _bus = bus;
            _logger = logger;
        }

        public async Task<bool> SendProcessUpdate(string authToken, string channelName, object data) {
            var message = new ProcessingUpdateMessage {
                UserId = authToken,
                ChannelName = channelName,
                Data = data
            };
            await _bus.PubSub.PublishAsync(message).ContinueWith(task => {
                if (task.IsCompleted && !task.IsFaulted) {
                    _logger.LogDebug("Successfully sent custom notification");
                }

                if (task.IsFaulted) {
                    _logger.LogError($"Unable to publish custom notification.\n{task.Exception}");
                }
            });
            return true;
        }
    }
}
