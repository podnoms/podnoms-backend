using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PodNoms.Api.Controllers {
    [Route("[controller]")]
    public class HooksController : BaseController {
        public HooksController(ILogger<HooksController> logger) : base(logger) {
        }

        [HttpPost("coinbase")]
        public async Task<ActionResult> CoinbasePayment() {
            using (var reader = new StreamReader(Request.Body, Encoding.UTF8)) {
                var body = await reader.ReadToEndAsync();
                dynamic hook = JsonConvert.DeserializeObject(body);
                _logger.LogDebug(body);
                // Do something
            }
            return Ok("Hello, Coinbase!");
        }
    }
}
