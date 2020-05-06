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
        public async Task<ContentResult> Get([FromQuery]string key) {
            var html = await ResourceReader.ReadResource($"{key}.html");
            return this.Content(html, "text/html");
        }
    }
}
