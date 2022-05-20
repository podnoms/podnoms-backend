﻿using System;
using System.Threading.Tasks;
using EasyNetQ;
using Microsoft.Extensions.Logging;
using PodNoms.Common.Data.Messages;
using PodNoms.Common.Services.Jobs;
using PodNoms.Data.Models;

namespace PodNoms.Jobs.Services {
    public class RabbitMqNotificationService : INotifyJobCompleteService {
        private readonly ILogger _logger;
        private readonly IBus _bus;

        public RabbitMqNotificationService(IBus bus, ILogger<RabbitMqNotificationService> logger) {
            _logger = logger;
            _bus = bus;
        }

        public async Task<bool> NotifyUser(string userId, string title, string body, string target, string image,
            NotificationOptions notificationType) {
            _logger.LogDebug($"Notifying user");
            var message = new NotifyUserMessage {
                UserId = userId,
                Title = title,
                Body = body,
                Target = target,
                Image = image
            };
            await _bus.PubSub.PublishAsync(message).ContinueWith(task => {
                if (task.IsFaulted) {
                    _logger.LogError($"Unable to publish custom notification.\n{task.Exception}");
                }
            });
            return true;
        }

        public async Task<bool> SendCustomNotifications(Guid podcastId, string userName, string title, string body,
            string url) {
            _logger.LogDebug($"Sending custom notification");
            var message = new CustomNotificationMessage {
                PodcastId = podcastId,
                Title = title,
                Body = body,
                Url = url
            };
            await _bus.PubSub.PublishAsync(message)
                .ContinueWith(task => {
                    if (task.IsFaulted) {
                        _logger.LogError("Unable to publish custom notification.\n{Exception}", task.Exception);
                    }
                });
            return true;
        }
    }
}
