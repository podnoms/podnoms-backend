using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace PodNoms.Common.Services.Hubs {
    [Authorize]
    public class AudioProcessingHub : Hub {
        public override async Task OnConnectedAsync() {
            await base.OnConnectedAsync();
        }
        public override async Task OnDisconnectedAsync(Exception exception) {
            await base.OnDisconnectedAsync(exception);
        }
    }
}