using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using EasyNetQ;
using EasyNetQ.AutoSubscribe;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PodNoms.Common.Data.Messages;
using PodNoms.Common.Services.Jobs;
using PodNoms.Data.Models;

namespace PodNoms.Common.Services.Hosted {
    public class RabbitMQService : BackgroundService {
        private readonly IBus _bus;
        private readonly ILogger<RabbitMQService> _logger;
        private readonly INotifyJobCompleteService _jobCompleteNotificationService;
        private readonly AutoSubscriber _subscriber;

        public RabbitMQService(IBus bus, IServiceScopeFactory serviceScopeFactory, ILogger<RabbitMQService> logger) {
            _bus = bus;
            _logger = logger;
            using (var scope = serviceScopeFactory.CreateScope()) {
                _jobCompleteNotificationService =
                    scope.ServiceProvider.GetRequiredService<INotifyJobCompleteService>();
            }
            _subscriber = new AutoSubscriber(_bus, "my_applications_subscriptionId_prefix");
            _subscriber.Subscribe(Assembly.GetExecutingAssembly());
            _bus.Subscribe<NotifyUserMessage>(
                "podnoms_message_notifyuser",
                message => {
                    Console.WriteLine($"(RabbitMQService) Consuming: {message.Body}");
                    _jobCompleteNotificationService.NotifyUser(
                        message.UserId,
                        message.Title,
                        message.Body,
                        message.Target,
                        message.Image, NotificationOptions.UploadCompleted);
                }
            );
            _bus.Subscribe<CustomNotificationMessage>(
               "podnoms_message_customnotification",
               message => {
                   _logger.LogDebug($"(RabbitMQService) Consuming: {message.Body}");
                   _jobCompleteNotificationService.SendCustomNotifications(
                       message.PodcastId,
                       "YOU NEED TO CHANGE THIS",
                       "PodNoms",
                       $"{message.Title} has finished processing",
                       message.Url);
               }
            );
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            stoppingToken.Register(() => Console.WriteLine("RabbitMQService is stopping."));
            while (!stoppingToken.IsCancellationRequested) {
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
            _bus.Dispose();
        }
    }
}
