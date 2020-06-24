using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace PodNoms.Api.Controllers {
    [Route("[controller]")]
    [EnableCors("DefaultPolicy")]
    public class PingController : Controller {
        [HttpGet]
        public string Get() {
            return "Pong";
        }
    }
}
