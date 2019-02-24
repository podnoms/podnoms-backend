using Microsoft.AspNetCore.Mvc;

namespace PodNoms.Api.Controllers.Public {
    [Route("pub/[controller]")]
    public class TestRoutingController : Controller {

        public ActionResult Index() {
            return View();
        }
    }
}