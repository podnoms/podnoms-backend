using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PodNoms.Data.Models;
using PodNoms.Common.Persistence;
using PodNoms.Common.Persistence.Repositories;
using System.Threading.Tasks;
using PodNoms.Common.Data.ViewModels.Resources;
using PodNoms.Common.Utils;
using Microsoft.Extensions.Options;
using PodNoms.Common.Data.Settings;

namespace PodNoms.Api.Controllers {
    [Route("[controller]")]
    public class WaveformController : BaseAuthController {
        private readonly IRepoAccessor _repo;
        private readonly StorageSettings _storageSettings;
        private readonly WaveformDataFileStorageSettings _waveformStorageSettings;

        public WaveformController(
            IHttpContextAccessor contextAccessor,
            UserManager<ApplicationUser> userManager,
            IRepoAccessor repo,
            IOptions<StorageSettings> storageSettings,
            IOptions<WaveformDataFileStorageSettings> waveformStorageSettings,
            ILogger<WaveformController> logger) : base(contextAccessor,
            userManager, logger) {
            _repo = repo;
            _storageSettings = storageSettings.Value;
            _waveformStorageSettings = waveformStorageSettings.Value;
        }

        [HttpGet("{entryId}")]
        public async Task<ActionResult<WaveformViewModel>> Get(string entryId) {
            var entry = await _repo.Entries.GetAsync(entryId);
            if (entry != null) {
                //offload the downloading of the data to the client for now, 
                //no need for us to be doing this heavy lifting 
                // var pcm = await HttpUtils.DownloadText(url, "application/json");
                return Ok(new WaveformViewModel {
                    PeakDataJsonUrl = Flurl.Url.Combine(
                        _storageSettings.CdnUrl,
                        _waveformStorageSettings.ContainerName,
                        $"{entry.Id}.json"),
                    PeakDataUrl = Flurl.Url.Combine(
                        _storageSettings.CdnUrl,
                        _waveformStorageSettings.ContainerName,
                        $"{entry.Id}.json"),
                });
            }

            return NotFound();
        }
    }
}
