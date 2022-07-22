using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using PodNoms.Common.Services.Jobs;
using PodNoms.Common.Services.Processor;
using PodNoms.Common.Utils.RemoteParsers;

namespace PodNoms.Api.Controllers.Public {
    [Route("pub/process")]
    public class PublicProcessorController : Controller {
        private readonly IUrlProcessService _processService;

        public PublicProcessorController(IUrlProcessService processService) {
            _processService = processService;
        }

        [HttpGet("validate")]
        public async Task<ActionResult<RemoteUrlStatus>> ValidateUrl([FromQuery] string url) {
            try {
                var result = await _processService.ValidateUrl(url, "PUBLICAPIREQUEST");
                return Ok(result);
            } catch (UrlParseException) {
                return NoContent();
            }
        }

        [HttpGet("process")]
        public ActionResult ProcessUrl([FromQuery] string url) {
            try {
                var updateId = System.Guid.NewGuid().ToString();
                var updateChannelId = $"{updateId}--processing";
                BackgroundJob.Enqueue<ConvertUrlToMp3Service>(r => r.ProcessEntry(url, updateId, null));

                return Ok(new {
                    updateChannelId = updateChannelId
                });
            } catch (UrlParseException) {
                return NoContent();
            }
        }
    }
}
