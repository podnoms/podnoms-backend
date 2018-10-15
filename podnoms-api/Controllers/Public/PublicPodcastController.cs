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
    [Route("pub/podcast")]
    public class PublicPodcastController : Controller {
        private readonly IPodcastRepository _podcastRepository;
        private readonly IMapper _mapper;

        public PublicPodcastController(IPodcastRepository podcastRepository, IMapper mapper) {
            _podcastRepository = podcastRepository;
            _mapper = mapper;
        }
        [HttpGet("{user}/{podcast}")]
        public async Task<ActionResult<List<PodcastViewModel>>> Get(string user, string podcast) {
            var results = await _podcastRepository.GetAll()
                .Where(r => r.AppUser.Slug == user && r.Slug == podcast)
                .Include(p => p.PodcastEntries)
                .ToListAsync();
            return _mapper.Map<List<Podcast>, List<PodcastViewModel>>(results);
        }
    }
}
