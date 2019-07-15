using EasyNetQ.AutoSubscribe;
using Microsoft.Extensions.Logging;
using PodNoms.Common.Data.Messages;
using PodNoms.Common.Services.Jobs;

namespace PodNoms.Common.Services.Hosted {
    public class RabbitMqConsumer : IConsume<NotifyUserMessage>, IConsume<CustomNotificationMessage> {
        private readonly INotifyJobCompleteService _jobCompleteNotificationService;
        private readonly ILogger _logger;

        RabbitMqConsumer(INotifyJobCompleteService jobCompleteNotificationService,
                         ILogger<RabbitMqConsumer> logger) {
            _jobCompleteNotificationService = jobCompleteNotificationService;
            _logger = logger;
        }
        public void Consume(NotifyUserMessage message) {
            _logger.LogDebug($"(RabbitMqConsumer) Consuming: {message.Body}");
            _jobCompleteNotificationService.NotifyUser(
                message.UserId,
                "PodNoms",
                $"{message.Title} has finished processing",
                message.Target,
                message.Image);
        }

        public void Consume(CustomNotificationMessage message) {
            _logger.LogDebug($"(RabbitMqConsumer) Consuming: {message.Body}");
            _jobCompleteNotificationService.SendCustomNotifications(
                message.PodcastId,
                "PodNoms",
                $"{message.Title} has finished processing",
               message.Url);
        }
    }
}
