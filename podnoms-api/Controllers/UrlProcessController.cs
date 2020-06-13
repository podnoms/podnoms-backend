using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PodNoms.Common.Services.Downloader;
using PodNoms.Common.Services.PageParser;
using PodNoms.Common.Services.Processor;
using PodNoms.Common.Utils;
using PodNoms.Common.Utils.RemoteParsers;
using PodNoms.Data.Models;
using System.Linq;
using System.Threading.Tasks;

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

        [HttpGet("_v")]
        [AllowAnonymous]
        public async Task<ActionResult> ValidateUrlPublic([FromQuery] string url) {
            return await ValidateUrl(url);
        }

        [HttpGet("validate")]
        [Authorize(AuthenticationSchemes = "Bearer, PodNomsApiKey")]
        [EnableCors("BrowserExtensionPolicy")]
        public async Task<ActionResult> ValidateUrl([FromQuery] string url) {
            if (string.IsNullOrEmpty(url) || !url.ValidateAsUrl()) {
                return Ok();
            }

            _logger.LogInformation($"Validating Url: {url}");
            var fileType = await _downloader.GetInfo(url);

            if (fileType == RemoteUrlType.Invalid) {
                if (!await _parser.Initialise(url)) {
                    return NoContent();
                }
                var title = _parser.GetPageTitle();
                var image = _parser.GetHeadTag("og:image");
                var description = _parser.GetHeadTag("og:description");

                var links = await _parser.GetAllAudioLinks();
                if (links.Count > 0) {
                    return new OkObjectResult(new {
                        type = "proxied",
                        title,
                        image,
                        description,
                        links = links
                            .GroupBy(r => r.Key)     // note to future me
                            .Select(g => g.First())  // these lines dedupe on key - neato!!
                            .Select(r => new {
                                key = r.Key,
                                value = r.Value
                            })
                    });
                }
                return Ok();
            }

            return new OkObjectResult(new {
                type = "native",
                title = _downloader.Properties?.Title,
                image = _downloader.Properties?.Thumbnail,
                description = _downloader.Properties?.Description,
                links = new[]  {
                    new {
                        title = _downloader.Properties?.Title,
                        key = url,
                        value = url
                    }
                }
            });

        }
    }
}
