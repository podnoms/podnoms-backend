using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using PodNoms.Common.Services.Hubs;

namespace PodNoms.Common.Services.Realtime {
    public class SignalRUpdater : IRealTimeUpdater {
        private readonly HubLifetimeManager<AudioProcessingHub> _hub;

        public SignalRUpdater(HubLifetimeManager<AudioProcessingHub> hub) {
            _hub = hub;
        }

        public async Task<bool> SendProcessUpdate(string userId, string channelName, object data) {
            try {
                await _hub.SendUserAsync(
                    userId,
                    channelName, //userId, 
                    new object[] {data});
                return true;
            } catch (Exception ex) {
                return false;
            }
        }
    }
}
