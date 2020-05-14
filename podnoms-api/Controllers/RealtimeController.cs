using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using PodNoms.Common.Persistence;
using PodNoms.Common.Services.Hubs;
using PodNoms.Data.ViewModels;

namespace PodNoms.Api.Controllers {
    [Route("[controller]")]
    public class RealtimeController : BaseController {
        private readonly PodNomsDbContext _context;
        private readonly HubLifetimeManager<EntityUpdatesHub> _hub;

        public RealtimeController(
                    ILogger<RealtimeController> logger,
                    HubLifetimeManager<EntityUpdatesHub> hub,
                    PodNomsDbContext context) : base(logger) {
            _hub = hub;
        }

        [HttpPost("update/{userId}")]
        public async Task<IActionResult> NotifyEntityUpdate(string userId, [FromBody] RealtimeEntityUpdateMessage message) {
            _logger.LogDebug($"UserId: {userId}\nMessage: {message.Channel}");
            await _hub.SendUserAsync(userId, message.Channel, new object[] { message });
            return Ok(message);
        }
    }
}
