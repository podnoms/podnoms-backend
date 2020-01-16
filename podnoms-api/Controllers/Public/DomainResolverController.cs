using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PodNoms.Common.Data.ViewModels;
using PodNoms.Common.Data.ViewModels.Resources;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Data.Models;

namespace PodNoms.Api.Controllers.Public {
    [Route("pub/podcast/domainresolver")]
    [EnableCors("PublicApiPolicy")]
    public class DomainResolverController : Controller {
        private readonly IPodcastRepository _podcastRepository;

        public DomainResolverController(IPodcastRepository podcastRepository) {
            _podcastRepository = podcastRepository;
        }
        [HttpGet]
        public async Task<ActionResult<PublicDomainViewModel>> ResolveDomain([FromQuery]string domain) {
            var cleanedDomain = domain.Split(":")[0]; //remove port
            var podcastUrl = string.Empty;
            var podcast = await _podcastRepository
                .GetAll()
                .Include(p => p.AppUser)
                .SingleOrDefaultAsync(r => r.CustomDomain == cleanedDomain);
            if (podcast != null) {
                podcastUrl = Flurl.Url.Combine(podcast.AppUser.Slug, podcast.Slug);
                return Ok(new PublicDomainViewModel {
                    PodcastId = podcast.Id.ToString(),
                    PodcastSlug = podcast.Slug,
                    UserSlug = podcast.AppUser.Slug,
                    Url = podcastUrl
                });
            }
            return NotFound();
        }
    }
}
