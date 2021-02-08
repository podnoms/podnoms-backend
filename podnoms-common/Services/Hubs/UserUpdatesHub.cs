using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.SignalR;

namespace PodNoms.Common.Services.Hubs {
    [EnableCors("PodNomsClientPolicy")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class UserUpdatesHub : Hub {
        public class UserUpdateMessage {
            public string Title { get; set; }
            public string Message { get; set; }
            public string ImageUrl { get; set; }
        }

        public async Task SendUserUpdate(string channelName, string eventName, object data) {
            var bus = $"{channelName}__{eventName}";
            await Clients.User("fergal.moran+podnoms@gmail.com").SendAsync(bus, data.ToString());
            await Clients.All.SendAsync(bus, data.ToString());
        }
    }
}
