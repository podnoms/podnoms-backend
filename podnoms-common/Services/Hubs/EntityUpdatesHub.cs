using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace PodNoms.Common.Services.Hubs {
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class EntityUpdatesHub : Hub {

    }
}
