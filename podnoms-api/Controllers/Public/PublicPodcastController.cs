using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PodNoms.Common.Data.ViewModels.Resources;
using PodNoms.Common.Persistence;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Data.Models;

namespace PodNoms.Api.Controllers.Public {
    [Route("pub/podcast")]
    [EnableCors("DefaultCors")]
    public class PublicPodcastController : Controller {
        private readonly IRepoAccessor _repo;
        private readonly IMapper _mapper;

        public PublicPodcastController(IRepoAccessor repo,
            IMapper mapper) {
            _repo = repo;
            _mapper = mapper;
        }

        [HttpGet("{user}/{podcast}")]
        public async Task<ActionResult<PodcastViewModel>> Get(string user, string podcast) {
            var result = await _repo.Podcasts.GetForUserAndSlugAsync(user, podcast);

            if (result is null) return NotFound();

            return _mapper.Map<Podcast, PodcastViewModel>(result);
        }

        [HttpGet("{userSlug}/{podcastSlug}/featured")]
        public async Task<ActionResult<PodcastEntryViewModel>> GetFeaturedEpisode(string userSlug, string podcastSlug) {
            var podcast = await _repo.Podcasts.GetAll()
                .OrderByDescending(p => p.CreateDate)
                .Include(p => p.PodcastEntries)
                .Include(p => p.AppUser)
                .SingleOrDefaultAsync(r => r.AppUser.Slug == userSlug && r.Slug == podcastSlug);

            var result = await _repo.Entries.GetFeaturedEpisode(podcast);
            if (result is null) return NotFound();

            return _mapper.Map<PodcastEntry, PodcastEntryViewModel>(result);
        }

        [HttpGet("{podcastId}/featured")]
        public async Task<ActionResult<PodcastEntryViewModel>> GetFeaturedEpisode(string podcastId) {
            //TODO: This should definitely have its own ViewModel
            var podcast = await _repo.Podcasts.GetAsync(podcastId);
            if (podcast is null) return NotFound();

            var result = await _repo.Entries.GetFeaturedEpisode(podcast);
            if (result is null) return NotFound();

            return _mapper.Map<PodcastEntry, PodcastEntryViewModel>(result);
        }

        [HttpGet("{podcastId}/allbutfeatured")]
        public async Task<ActionResult<List<PodcastEntryViewModel>>> GetAllButFeatured(string podcastId) {
            var podcast = await _repo.Podcasts.GetAsync(podcastId);
            if (podcast is null) return NotFound();

            var result = await _repo.Entries.GetAllButFeatured(podcast);
            if (result is null) return NotFound();

            return _mapper.Map<List<PodcastEntry>, List<PodcastEntryViewModel>>(result);
        }

        [HttpGet("{podcastId}/aggregators")]
        public async Task<ActionResult<List<PodcastAggregator>>> GetAggregators(string podcastId,
            [FromQuery] string type = "") {
            //TODO: This should definitely have its own ViewModel
            var aggregators = (await _repo.Podcasts.GetAggregators(Guid.Parse(podcastId)));
            if (aggregators is null) return NotFound();
            if (!string.IsNullOrEmpty(type)) {
                aggregators = aggregators
                    .Where(a => a.Name.Equals(type))
                    .ToList();
            }

            return Ok(aggregators.Select(a => new {
                a.Url,
                a.ImageUrl,
                a.Name
            }));
        }
    }
}
