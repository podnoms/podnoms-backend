using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PodNoms.Common.Data.ViewModels;
using PodNoms.Common.Data.ViewModels.Resources;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Data.Models;

namespace PodNoms.Api.Controllers.Public {
    [Route("pub/podcast/domainresolver")]
    [EnableCors("PublicApiPolicy")]
    public class DomainResolverController : Controller {
        private readonly ILogger<DomainResolverController> _logger;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IPodcastRepository _podcastRepository;

        public DomainResolverController(
                    ILogger<DomainResolverController> logger,
                    IHttpContextAccessor contextAccessor,
                    IPodcastRepository podcastRepository) {
            _logger = logger;
            _contextAccessor = contextAccessor;
            _podcastRepository = podcastRepository;
        }
        [HttpGet]
        public async Task<ActionResult<PublicDomainViewModel>> ResolveDomain() {
            var domain =
                _contextAccessor.HttpContext.Request.Headers["Origin"].FirstOrDefault() ??
                _contextAccessor.HttpContext.Request.Headers["Referer"].FirstOrDefault();

            _logger.LogDebug($"ResolveDomain: {domain}");
            if (!string.IsNullOrEmpty(domain)) {
                var cleanedDomain = new Uri(domain).Host;

                var podcastUrl = string.Empty;
                var podcast = await _podcastRepository
                    .GetAll()
                    .Include(p => p.AppUser)
                    .SingleOrDefaultAsync(r => r.CustomDomain == cleanedDomain);
                if (podcast != null) {
                    podcastUrl = Flurl.Url.Combine(podcast.AppUser.Slug, podcast.Slug);
                    return Ok(new PublicDomainViewModel {
                        Domain = cleanedDomain,
                        PodcastId = podcast.Id.ToString(),
                        PodcastSlug = podcast.Slug,
                        UserSlug = podcast.AppUser.Slug,
                        Url = podcastUrl
                    });
                }
            }
            return NotFound();
        }
    }
}
