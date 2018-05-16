using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PodNoms.Api.Services.Jobs;

namespace PodNoms.Api.Controllers {
    [Authorize]
    [Route("[controller]")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class JobController : BaseController {
        public JobController(ILogger<JobController> logger) : base(logger) {
        }

        [HttpGet("processorphans")]
        public IActionResult ProcessOrphans() {
            var infoJobId = BackgroundJob.Enqueue<ClearOrphanAudioJob>(service => service.Execute());
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
        [HttpGet("updateyoutubedl")]
        public IActionResult UpdateYouTubeDl() {
            var infoJobId = BackgroundJob.Enqueue<UpdateYouTubeDlJob>(service => service.Execute());
            return Ok();
        }
    }
}