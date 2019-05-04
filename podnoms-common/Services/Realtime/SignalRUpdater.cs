using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using PodNoms.Common.Services.Hubs;

namespace PodNoms.Common.Services.Realtime {
    public class SignalRUpdater : IRealTimeUpdater {
        private readonly HubLifetimeManager<AudioProcessingHub> _hub;
        public SignalRUpdater(HubLifetimeManager<AudioProcessingHub> hub) {
            _hub = hub;

        }
        public async Task<bool> SendProcessUpdate(string userId, string channelName, string eventName, object data) {
            var bus = $"{channelName}__{eventName}";
            await _hub.SendUserAsync(
                userId,
                bus, //userId, 
                new object[] { data });
            return true;
        }
    }
}