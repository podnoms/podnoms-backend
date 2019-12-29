using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        public async Task<ActionResult<string>> ResolveDomain([FromQuery]string domain) {
            var podcast = await _podcastRepository
                .GetAll()
                .Include(p => p.AppUser)
                .SingleAsync(r => r.CustomDomain == domain);
            if (podcast != null) {
                var customUrl = Flurl.Url.Combine(podcast.AppUser.Slug, podcast.Slug);
                return Ok(customUrl);
            }
            return NotFound();
        }
    }
}
