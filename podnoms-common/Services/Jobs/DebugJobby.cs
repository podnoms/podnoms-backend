using System;
using System.Threading.Tasks;
using EasyNetQ;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using PodNoms.Common.Messaging.Contracts;

namespace PodNoms.Common.Services.Jobs {
    public class DebugJobby : IHostedJob {
        private readonly IBus _bus;
        private readonly ILogger<DebugJobby> _logger;

        public DebugJobby(IBus job, ILogger<DebugJobby> logger) {
            this._logger = logger;
            this._bus = job;
        }

        public Task<bool> Execute() {
            return Execute(null);
        }

        public async Task<bool> Execute(PerformContext context) {
            var message = new NotifyUserMessage {
                UserId = "USERID",
                Title = "title",
                Body = "body",
                Target = "target",
                Image = "image"
            };
            await _bus.PublishAsync(message).ContinueWith(task => {
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
