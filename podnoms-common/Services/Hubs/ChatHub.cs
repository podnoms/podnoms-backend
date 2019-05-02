using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace PodNoms.Common.Services.Hubs {
    [Authorize]
    public class ChatHub : Hub {

        
    }
}