using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;

namespace PodNoms.Common.Services.Jobs {
    public class ClientHeartbeatJob : IJob {
        private readonly ILogger<ClientHeartbeatJob> _logger;

        public ClientHeartbeatJob(ILogger<ClientHeartbeatJob> logger) {
            this._logger = logger;
        }
        [AutomaticRetry(OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        public async Task<bool> Execute() { return await Execute(null); }

        [AutomaticRetry(OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        public async Task<bool> Execute(PerformContext context) {
            return await Task.Factory.StartNew(() => {
                _logger.LogDebug($"Heartbeat: {System.DateTime.Now.ToLocalTime()}");
                return true;
            });
        }
    }
}
