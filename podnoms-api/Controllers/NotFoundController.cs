using Microsoft.AspNetCore.Mvc;

namespace PodNoms.Api.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("404")]
    public class NotFoundController : Controller {
        public NotFoundController() {
        }

        public ActionResult Index() {
            return View();
        }
    }
}
