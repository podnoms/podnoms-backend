using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PodNoms.Common.Data.ViewModels.Resources;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Data.Models;

namespace PodNoms.Api.Controllers.Public {
    [Route("pub/entry")]
    public class PublicEntryController : Controller {
        private readonly IEntryRepository _entryRepository;
        private readonly IMapper _mapper;

        public PublicEntryController(IEntryRepository entryRepository,
            IMapper mapper) {
            _entryRepository = entryRepository;
            _mapper = mapper;
        }

        [HttpGet("{user}/{podcast}/{entry}")]
        public async Task<ActionResult<PodcastEntryViewModel>> Get(string user, string podcast, string entry) {
            var result = await _entryRepository.GetForUserAndPodcast(user, podcast, entry);

            if (result is null) return NotFound();

            return _mapper.Map<PodcastEntry, PodcastEntryViewModel>(result);
        }
    }
}
