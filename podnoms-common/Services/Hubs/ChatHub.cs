using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace PodNoms.Common.Services.Hubs {
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class ChatHub : Hub {

    }
}
