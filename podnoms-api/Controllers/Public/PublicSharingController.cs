using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PodNoms.Common.Data.ViewModels.Resources;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Data.Models;

namespace PodNoms.Api.Controllers.Public {
    [Route("pub/sharing")]
    public class PublicSharingController : Controller {
        private readonly IEntryRepository _entryRepository;
        private readonly IMapper _mapper;

        public PublicSharingController(IEntryRepository entryRepository,
            IMapper mapper) {
            _entryRepository = entryRepository;
            _mapper = mapper;
        }

        [AllowAnonymous]
        [HttpGet("{shareId}")]
        public async Task<IActionResult> Index(string shareId) {
            var entry = await this._entryRepository.GetEntryForShareId(shareId);
            if (entry != null)
                return View(_mapper.Map<PodcastEntry, SharingPublicViewModel>(entry));

            return NotFound();
        }
    }
}