using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace PodNoms.Api.Controllers.WebHooks {
    [AllowAnonymous]
    [Route("/hooks/patreon")]
    public class PatreonWebHookController : BaseController {
        public PatreonWebHookController(ILogger<PatreonWebHookController> logger) : base(logger) {
        }

        [HttpGet("0a5d7a16-0335-47ad-8071-8667634947fd")]
        public ActionResult Get([FromQuery] string message) {
            return Ok(message);
        }
    }
}
