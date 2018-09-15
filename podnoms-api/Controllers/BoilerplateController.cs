using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PodNoms.Common.Utils;

namespace PodNoms.Api.Controllers {
    [Route("[controller]")]
    public class BoilerplateController : BaseController {
        public BoilerplateController(ILogger<BoilerplateController> logger) : base(logger) {
        }

        [HttpGet]
        public async Task<ActionResult<string>> Get(string key) {
            var html = await ResourceReader.ReadResource($"{key}.html");
            return Content(html, "text/plain");
        }
    }
}