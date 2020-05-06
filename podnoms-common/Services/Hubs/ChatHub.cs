using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace PodNoms.Common.Services.Hubs {
    //this needs to allow anonymous access
    // [Authorize(AuthenticationSchemes = "Bearer")]
    public class ChatHub : Hub {

    }
}
