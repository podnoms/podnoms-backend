using System;
using System.Threading.Tasks;
using EasyNetQ;
using Hangfire.Server;
using Microsoft.Extensions.Logging;

namespace PodNoms.Common.Services.Jobs {
    public class DebugJobby : IHostedJob {
        private readonly IBus _bus;
        private readonly ILogger<DebugJobby> _logger;

        public DebugJobby(IBus job, ILogger<DebugJobby> logger) {
            this._logger = logger;
            this._bus = job;
        }

        public async Task<bool> Execute() {
            return await Execute(null);
        }

        public async Task<bool> Execute(PerformContext context) {
            return await Task.Factory.StartNew(() => {
                Guid playlistId = Guid.Parse("544e9984-7ed5-4c76-10e6-08d70ff62e10");
                // BackgroundJob.Enqueue<ProcessPlaylistsJob>(r => r.Execute(playlistId, context));
                return true;
            });
        }
    }
}
