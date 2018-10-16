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
        public async Task<ActionResult<PodcastViewModel>> Get(string user, string podcast) {
            var results = await _podcastRepository.GetAll()
                .Include(p => p.PodcastEntries)
                .SingleAsync(r => r.AppUser.Slug == user && r.Slug == podcast);
            return _mapper.Map<Podcast, PodcastViewModel>(results);
        }
    }
}
