using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PodNoms.Common.Data.Settings;
using PodNoms.Common.Data.ViewModels.Resources;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Data.Models;

namespace PodNoms.Api.Controllers.Public {
    [Route("pub/sharing")]
    public class PublicSharingController : Controller {
        private readonly IEntryRepository _entryRepository;
        private readonly StorageSettings _storageSettings;
        private readonly WaveformDataFileStorageSettings _waveformStorageSettings;
        private readonly SharingSettings _sharingSettings;
        private readonly IMapper _mapper;

        public PublicSharingController(IEntryRepository entryRepository,
                        IOptions<StorageSettings> storageSettings,
                        IOptions<WaveformDataFileStorageSettings> waveformStorageSettings,
                        IOptions<SharingSettings> sharingSettings,
            IMapper mapper) {
            _entryRepository = entryRepository;
            _storageSettings = storageSettings.Value;
            _waveformStorageSettings = waveformStorageSettings.Value;
            _sharingSettings = sharingSettings.Value;
            _mapper = mapper;
        }

        [AllowAnonymous]
        [HttpGet("{shareId}")]
        public async Task<ActionResult<PublicSharingViewModel>> Index(string shareId) {
            var entry = await this._entryRepository.GetEntryForShareId(shareId);
            if (entry != null) {
                var model = _mapper.Map<PodcastEntry, PublicSharingViewModel>(entry);
                model.Url = Flurl.Url.Combine(_sharingSettings.BaseUrl, shareId);
                model.PeakDataUrl = Flurl.Url.Combine(
                    _storageSettings.CdnUrl,
                    _waveformStorageSettings.ContainerName,
                    $"{entry.Id}.json");
                return View(model);
            }
            return NotFound();
        }
        [AllowAnonymous]
        [HttpGet("details/{shareId}")]
        public async Task<ActionResult<PublicSharingViewModel>> GetDetails(string shareId) {
            var entry = await this._entryRepository.GetEntryForShareId(shareId);
            if (entry != null)
                return Ok(_mapper.Map<PodcastEntry, PublicSharingViewModel>(entry));

            return NotFound();
        }

    }
}
