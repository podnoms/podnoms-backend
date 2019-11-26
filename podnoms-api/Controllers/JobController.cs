using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PodNoms.Common.Services.Jobs;

namespace PodNoms.Api.Controllers {
    [Authorize]
    [Route("[controller]")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class JobController : BaseController {
        public JobController(ILogger<JobController> logger) : base(logger) {
        }

        [HttpGet("deleteorphans")]
        public IActionResult DeleteOrphans() {
            var infoJobId = BackgroundJob.Enqueue<DeleteOrphanAudioJob>(service => service.Execute());
            return Ok();
        }
        [HttpGet("processplaylists")]
        public IActionResult ProcessPlaylists() {
            var infoJobId = BackgroundJob.Enqueue<ProcessPlaylistsJob>(service => service.Execute());
            return Ok();
        }
        [HttpGet("processplaylistitems")]
        public IActionResult ProcessPlaylistItems() {
            var infoJobId = BackgroundJob.Enqueue<ProcessPlaylistItemJob>(service => service.Execute());
            return Ok();
        }
        [HttpGet("processmissing")]
        public IActionResult ProcessMissingItems() {
            var infoJobId = BackgroundJob.Enqueue<ProcessMissingPodcastsJob>(service => service.Execute());
            return Ok();
        }
        [HttpGet("updateimages")]
        public IActionResult UpdateImages() {
            var infoJobId = BackgroundJob.Enqueue<CacheRemoteImageJob>(service => service.Execute());
            return Ok();
        }
        [HttpGet("updateyoutubedl")]
        public IActionResult UpdateYouTubeDl() {
            var infoJobId = BackgroundJob.Enqueue<UpdateYouTubeDlJob>(service => service.Execute());
            return Ok();
        }
        [HttpGet("processpodcastjob")]
        public IActionResult UpdateYouTubeDl([FromQuery]string entryId) {
            _logger.LogDebug($"Creating job for {entryId}");
            var infoJobId = BackgroundJob.Enqueue<ProcessMissingPodcastsJob>(
                service => service.ExecuteForEntry(entryId, null)
            );
            return Ok();
        }
    }
}
