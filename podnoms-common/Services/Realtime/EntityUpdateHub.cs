using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace PodNoms.Common.Services.Realtime {
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class EntityUpdateHub : Hub {

    }
}
