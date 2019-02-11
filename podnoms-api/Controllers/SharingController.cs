using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PodNoms.Common.Data.ViewModels.Resources;
using PodNoms.Common.Persistence;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Data.Models;

namespace PodNoms.Api.Controllers {
    [Route("[controller]")]
    public class SharingController : BaseAuthController {
        private readonly IEntryRepository _entryRepository;
        private readonly IUnitOfWork _unitOfWork;

        public SharingController(
            IHttpContextAccessor contextAccessor,
            UserManager<ApplicationUser> userManager,
            IEntryRepository entryRepository,
            IUnitOfWork unitOfWork,
            ILogger<SharingController> logger) : base(contextAccessor, userManager, logger) {
            this._entryRepository = entryRepository;
            this._unitOfWork = unitOfWork;
        }

        [HttpPost]
        [Produces("text/plain")]
        public async Task<IActionResult> GenerateSharingLink([FromBody] SharingViewModel model) {
            var entry = await _entryRepository.GetAsync(_applicationUser.Id, model.Id);

            if (entry == null)
                return NotFound();

            var share = await _entryRepository.CreateNewSharingLink(model);
            if (share != null){
                await _unitOfWork.CompleteAsync();
                return Ok(share);
            }

            return BadRequest();
        }
    }
}
