using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PodNoms.Common.Data.Settings;
using PodNoms.Common.Data.ViewModels.Resources;
using PodNoms.Common.Persistence;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Common.Services.Startup;
using PodNoms.Common.Utils;
using PodNoms.Data.Models;

namespace PodNoms.Api.Controllers {
    [Route("[controller]")]
    public class SharingController : BaseAuthController {
        private readonly IEntryRepository _entryRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly SharingSettings _sharingSettings;

        public SharingController(
            IHttpContextAccessor contextAccessor,
            UserManager<ApplicationUser> userManager,
            IEntryRepository entryRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IOptions<SharingSettings> sharingSettings,
            ILogger<SharingController> logger) : base(contextAccessor, userManager, logger) {
            this._entryRepository = entryRepository;
            this._unitOfWork = unitOfWork;
            this._mapper = mapper;
            this._sharingSettings = sharingSettings.Value;
        }

        [HttpPost]
        public async Task<ActionResult<SharingResultViewModel>> GenerateSharingLink([FromBody] SharingViewModel model) {
            var entry = await _entryRepository.GetAsync(_applicationUser.Id, model.Id);
            if (entry == null)
                return NotFound();

            var share = await _entryRepository.CreateNewSharingLink(model);
            if (share != null) {
                await _unitOfWork.CompleteAsync();
                return Ok(new SharingResultViewModel {
                    Id = model.Id,
                    ValidFrom = model.ValidFrom,
                    ValidTo = model.ValidTo,
                    Url = Flurl.Url.Combine(new string[] {_sharingSettings.BaseUrl, share.LinkId})
                });
            }

            return BadRequest();
        }

    }
}