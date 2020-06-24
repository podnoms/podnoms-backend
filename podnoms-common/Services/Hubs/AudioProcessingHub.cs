using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.SignalR;

namespace PodNoms.Common.Services.Hubs {
    [EnableCors("PodNomsClientPolicy")]
    [Authorize(AuthenticationSchemes = "Bearer, PodNomsApiKey")]
    public class AudioProcessingHub : Hub {
        public override async Task OnConnectedAsync() {
            await base.OnConnectedAsync();
        }
        public override async Task OnDisconnectedAsync(Exception exception) {
            await base.OnDisconnectedAsync(exception);
        }
        public async Task Send(string channelName, object data) {
            if (Context.User.Identity.IsAuthenticated) {
                var claimsIdentity = (ClaimsIdentity)Context.User.Identity;
                var userIdClaim = claimsIdentity.Claims.SingleOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
                if (userIdClaim != null) {
                    await Clients.User(userIdClaim.Value).SendAsync(channelName, data.ToString());
                    await Clients.All.SendAsync(channelName, data);
                }
            }
        }
    }
}
