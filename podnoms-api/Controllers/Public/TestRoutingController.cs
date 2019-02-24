using Microsoft.AspNetCore.Mvc;

namespace PodNoms.Api.Controllers.Public {
    [Route("pub/sharing/[controller]")]
    public class TestRoutingController : Controller {

        public ActionResult Index() {
            return View();
        }
    }
}