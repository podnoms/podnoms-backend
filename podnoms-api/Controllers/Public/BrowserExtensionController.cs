using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using PodNoms.Common.Services.PageParser;

namespace PodNoms.Api.Controllers.Public {
    [Route ("pub/browserextension")]
    public class BrowserExtensionController : Controller {
        private readonly IPageParser _parser;

        public BrowserExtensionController (IPageParser parser) {
            this._parser = parser;
        }

        [HttpGet ("parse")]
        [EnableCors ("BrowserExtensionPolicy")]
        public async Task<IActionResult> ParsePage ([FromQuery] string url) {
            await _parser.Initialise (url);
            var links = await _parser.GetAllAudioLinks ();
            if (links.Count > 0) {
                return new OkObjectResult (new {
                    type = "proxied",
                        links = links
                });
            }
            return BadRequest ();
        }
    }
}
