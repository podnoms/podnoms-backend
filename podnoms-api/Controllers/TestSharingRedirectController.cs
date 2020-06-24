using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PodNoms.Common.Data.Settings;
using PodNoms.Common.Data.ViewModels.Resources;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Data.Models;

namespace PodNoms.Api.Controllers {
    [Route("[controller]")]
    public class TestSharingRedirectController : BaseController {
        private readonly IEntryRepository _entryRepository;
        private readonly StorageSettings _storageSettings;
        private readonly WaveformDataFileStorageSettings _waveformStorageSettings;
        private readonly SharingSettings _sharingSettings;
        private readonly IMapper _mapper;

        public TestSharingRedirectController(IEntryRepository entryRepository,
                        IOptions<StorageSettings> storageSettings,
                        IOptions<WaveformDataFileStorageSettings> waveformStorageSettings,
                        IOptions<SharingSettings> sharingSettings,
                        ILogger<TestSharingRedirectController> logger,
                        IMapper mapper) : base(logger) {

            _entryRepository = entryRepository;
            _storageSettings = storageSettings.Value;
            _waveformStorageSettings = waveformStorageSettings.Value;
            _sharingSettings = sharingSettings.Value;
            _mapper = mapper;
        }

        [AllowAnonymous]
        [HttpGet("{shareId}")]
        public async Task<ActionResult<PublicSharingViewModel>> Get(string shareId) {
            var entry = await this._entryRepository.GetEntryForShareId(shareId);
            if (entry == null) {
                return NotFound();
            }

            var model = _mapper.Map<PodcastEntry, PublicSharingViewModel>(entry);
            model.Url = Flurl.Url.Combine(_sharingSettings.BaseUrl, shareId);
            model.PeakDataUrl = Flurl.Url.Combine(
                _storageSettings.CdnUrl,
                _waveformStorageSettings.ContainerName,
                $"{entry.Id}.json");

            return View(model);
        }
    }
}
