using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace PodNoms.Common.Services.Jobs {
    public class ClientHeartbeatJob : IJob {
        private readonly ILogger<ClientHeartbeatJob> _logger;

        public ClientHeartbeatJob (ILogger<ClientHeartbeatJob> logger) {
            this._logger = logger;
        }
        public async Task<bool> Execute () {
            return await Task.Factory.StartNew (() => {
                _logger.LogDebug ($"Heartbeat: {System.DateTime.Now.ToLocalTime()}");
                return true;
            });
        }
    }
}
