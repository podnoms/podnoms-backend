using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.SignalR;

namespace PodNoms.Common.Services.Hubs {
    [EnableCors("PodNomsClientPolicy")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class EntityUpdatesHub : Hub {

    }
}
