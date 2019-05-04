using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace PodNoms.Common.Services.Hubs {


    [Authorize]
    public class UserUpdatesHub : Hub {
        public class UserUpdateMessage {
            public string Title { get; set; }
            public string Message { get; set; }
            public string ImageUrl { get; set; }
        }
    }
}