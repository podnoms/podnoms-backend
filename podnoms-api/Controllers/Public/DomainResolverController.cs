using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PodNoms.Common.Data.ViewModels;
using PodNoms.Common.Persistence;

namespace PodNoms.Api.Controllers.Public {
    [Route("pub/domainresolver")]
    [EnableCors("DefaultCors")]
    public class DomainResolverController : Controller {
        private readonly ILogger<DomainResolverController> _logger;
        private readonly IRepoAccessor _repo;

        public DomainResolverController(
            ILogger<DomainResolverController> logger,
            IRepoAccessor repo) {
            _logger = logger;
            _repo = repo;
        }

        [HttpGet]
        public async Task<ActionResult<PublicDomainViewModel>> ResolveDomain([FromQuery] string domain) {
            try {
                var cleanedDomain = domain.Split(":")[0]; //remove port
                var podcast = await _repo.Podcasts
                    .GetAll()
                    .Include(p => p.AppUser)
                    .SingleOrDefaultAsync(r => r.CustomDomain == cleanedDomain);
                if (podcast != null) {
                    var podcastUrl = Flurl.Url.Combine(podcast.AppUser.Slug, podcast.Slug);
                    return Ok(new PublicDomainViewModel {
                        Domain = cleanedDomain,
                        PodcastId = podcast.Id.ToString(),
                        PodcastSlug = podcast.Slug,
                        UserSlug = podcast.AppUser.Slug,
                        Url = podcastUrl
                    });
                }
            } catch (Exception e) {
                _logger.LogError("Error resolving custom domain: {Domain}\\n{Message}", domain, e.Message);
            }

            return Ok();
        }
    }
}
