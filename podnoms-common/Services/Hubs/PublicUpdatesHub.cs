using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.SignalR;

namespace PodNoms.Common.Services.Hubs {
    public class PublicUpdatesHub : Hub {
        public async Task SendMessage(string channelName, string eventName, object data) {
            var bus = $"{channelName}--{eventName}";
            await Clients.All.SendAsync(bus, data);
        }
    }
}
