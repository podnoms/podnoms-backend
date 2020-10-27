using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PodNoms.Common.Services.Downloader;
using PodNoms.Common.Services.PageParser;
using PodNoms.Common.Services.Processor;
using PodNoms.Common.Utils.RemoteParsers;
using PodNoms.Data.Models;
using System.Threading.Tasks;

namespace PodNoms.Api.Controllers {
    [Route("[controller]")]
    [Authorize]
    public class UrlProcessController : BaseAuthController {
        private readonly IUrlProcessService _processService;
        private readonly AudioDownloader _downloader;

        public UrlProcessController(IHttpContextAccessor contextAccessor, UserManager<ApplicationUser> userManager,
            ILogger<UrlProcessController> logger, IUrlProcessService processService,
            AudioDownloader downloader) :
            base(contextAccessor, userManager, logger) {
            this._processService = processService;
            this._downloader = downloader;
        }

        [HttpGet("_v")]
        [AllowAnonymous]
        public async Task<ActionResult<RemoteUrlStatus>> ValidateUrlPublic([FromQuery] string url) {
            return await ValidateUrl(url);
        }

        [HttpGet("validate")]
        public async Task<ActionResult<RemoteUrlStatus>> ValidateUrl([FromQuery] string url) {
            try {
                var result = await _processService.ValidateUrl(url, UserId);
                return Ok(result);
            } catch (UrlParseException) {
                return NoContent();
            }
        }

        [HttpGet("process")]
        public async Task<ActionResult<RemoteUrlStatus>> ProcessUrl([FromQuery] string url) {
            try {
                var result = await _processService.ValidateUrl(url, UserId);
                return Ok(result);
            } catch (UrlParseException) {
                return NoContent();
            }
        }
    }
}
