﻿using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
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
        private readonly IPageParser _parser;
        private readonly HelpersSettings _helpersSettings;

        public UrlProcessController(IHttpContextAccessor contextAccessor, UserManager<ApplicationUser> userManager,
                            ILogger<UrlProcessController> logger, IUrlProcessService processService,
                            IPageParser parser, IOptions<HelpersSettings> helpersSettings) :
                            base(contextAccessor, userManager, logger) {
            this._processService = processService;
            this._parser = parser;
            _helpersSettings = helpersSettings.Value;
        }

        [HttpGet("validate")]
        public async Task<ActionResult> ValidateUrl([FromQuery]string url) {
            var downloader = new AudioDownloader(url, _helpersSettings.Downloader);
            var fileType = downloader.GetInfo();
            if (fileType == AudioType.Invalid) {
                var links = await _parser.GetAllAudioLinks(url);
                if (links.Count > 0) {
                    return new OkObjectResult(new {
                        type = "proxied",
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
