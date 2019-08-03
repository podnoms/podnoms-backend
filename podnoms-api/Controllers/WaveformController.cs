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
        private readonly IEntryRepository _entryRepository;
        private readonly StorageSettings _storageSettings;
        private readonly WaveformDataFileStorageSettings _waveformStorageSettings;

        public WaveformController(
                IEntryRepository entryRepository,
                IHttpContextAccessor contextAccessor,
                UserManager<ApplicationUser> userManager,
                IUnitOfWork unitOfWork,
                IOptions<StorageSettings> storageSettings,
                IOptions<WaveformDataFileStorageSettings> waveformStorageSettings,
                ILogger<WaveformController> logger) : base(contextAccessor,
            userManager, logger) {
            _entryRepository = entryRepository;
            _storageSettings = storageSettings.Value;
            _waveformStorageSettings = waveformStorageSettings.Value;
        }
        [HttpGet]
        public async Task<ActionResult<WaveformViewModel>> Get([FromQuery]string entryId) {
            var entry = await _entryRepository.GetAsync(entryId);
            if (entry != null) {
                //offload the downloading of the data to the client for now, 
                //no need for us to be doing this heavy lifting 
                // var pcm = await HttpUtils.DownloadText(url, "application/json");
                return Ok(new WaveformViewModel {
                    PeakDataJsonUrl = $"{_storageSettings.CdnUrl}{_waveformStorageSettings.ContainerName}/{entry.Id}.json",
                    PeakDataUrl = $"{_storageSettings.CdnUrl}{_waveformStorageSettings.ContainerName}/{entry.Id}.json",
                });
            }
            return NotFound();
        }
    }
}
