using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        [HttpGet("top100")]
        public async Task<ActionResult<List<PodcastEntryViewModel>>> Top100(string user, string podcast, string entry) {
            var results = await _entryRepository
                .GetAll()
                .OrderByDescending(r => r.CreateDate)
                .Take(100)
                .ToListAsync();

            return _mapper.Map<List<PodcastEntry>, List<PodcastEntryViewModel>>(results);
        }
        [HttpGet("{user}/{podcast}/{entry}")]
        public async Task<ActionResult<PodcastEntryViewModel>> Get(string user, string podcast, string entry) {
            var result = await _entryRepository.GetForUserAndPodcast(user, podcast, entry);

            if (result is null) return NotFound();

            return _mapper.Map<PodcastEntry, PodcastEntryViewModel>(result);
        }
    }
}
