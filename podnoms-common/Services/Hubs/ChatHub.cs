using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.SignalR;

namespace PodNoms.Common.Services.Hubs {
    public class ChatHub : Hub {
        public async Task Send(string channelName, object data) {
            await Clients.All.SendAsync(channelName, data);
        }
    }
}
