using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PodNoms.Common.Data.Settings;
using PodNoms.Common.Services;
using PodNoms.Common.Services.Downloader;
using PodNoms.Common.Services.PageParser;
using PodNoms.Common.Services.Processor;
using PodNoms.Data.Models;

namespace PodNoms.Api.Controllers {
    [Route("[controller]")]
    [Authorize]
    public class UrlProcessController : BaseAuthController {
        private readonly IUrlProcessService _processService;
        private readonly AudioDownloader _downloader;
        private readonly IPageParser _parser;

        public UrlProcessController(IHttpContextAccessor contextAccessor, UserManager<ApplicationUser> userManager,
                ILogger<UrlProcessController> logger, IUrlProcessService processService,
                AudioDownloader downloader, IPageParser parser) :
            base(contextAccessor, userManager, logger) {
            this._processService = processService;
            this._downloader = downloader;
            this._parser = parser;
        }

        [HttpGet("__temp__naked__validate")]
        [AllowAnonymous]
        [DisableCors]
        public async Task<ActionResult> ___ValidateUrl([FromQuery] string url) {
            await _parser.Initialise(url);
            var links = await _parser.GetAllAudioLinks();
            if (links.Count > 0) {
                return new OkObjectResult(new {
                    type = "proxied",
                    data = links
                });
            }
            return BadRequest();
        }

        [HttpGet("validate")]
        [AllowAnonymous]
        public async Task<ActionResult> ValidateUrl([FromQuery] string url) {
            var fileType = _downloader.GetInfo(url);

            if (fileType == AudioType.Invalid) {
                await _parser.Initialise(url);
                var title = _parser.GetPageTitle();
                var links = await _parser.GetAllAudioLinks();
                if (links.Count > 0) {
                    return new OkObjectResult(new {
                        type = "proxied",
                        title = title,
                        data = links
                    });
                }
                return BadRequest();
            }
            return new OkObjectResult(new {
                type = "native"
            });
        }
    }
}
