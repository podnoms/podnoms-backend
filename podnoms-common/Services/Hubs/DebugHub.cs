using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace PodNoms.Common.Services.Hubs {

    public class DebugHub : Hub {
        private readonly ILogger _logger;

        public DebugHub(ILogger<DebugHub> logger) {
            _logger = logger;
        }
        public async Task Send(string name, string message) {
            _logger.LogDebug($"New message For: {name} on hub: debug{Environment.NewLine}{message}");
            await Clients.All.SendAsync("send", name, message);
        }
        public async Task Ping() {
            await Clients.All.SendAsync("ping", new[] { "Pong", "Pong", "Pong" });
        }
    }
}
