using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HandlebarsDotNet;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PodNoms.Common.Auth;
using PodNoms.Common.Data.Settings;
using PodNoms.Common.Data.ViewModels.RssViewModels;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Common.Utils;
using PodNoms.Common.Utils.Extensions;
using PodNoms.Data.Models;

namespace PodNoms.Api.Controllers {
    [Route("[controller]")]
    public class RssController : Controller {
        private readonly IPodcastRepository _podcastRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRepository<ApplicationUserSlugRedirects> _redirectsRepository;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly ILogger _logger;
        private readonly AppSettings _appOptions;
        private readonly StorageSettings _storageOptions;
        private readonly ImageFileStorageSettings _imageOptions;

        public RssController(IPodcastRepository podcastRespository,
            IOptions<AppSettings> appOptions,
            IOptions<ImageFileStorageSettings> imageOptions,
            IOptions<StorageSettings> storageOptions,
            UserManager<ApplicationUser> userManager,
            IRepository<ApplicationUserSlugRedirects> redirectsRepository,
            IHttpContextAccessor contextAccessor,
            ILoggerFactory loggerFactory) {
            _podcastRepository = podcastRespository;
            _userManager = userManager;
            _redirectsRepository = redirectsRepository;
            _contextAccessor = contextAccessor;
            _appOptions = appOptions.Value;
            _imageOptions = imageOptions.Value;
            _storageOptions = storageOptions.Value;
            _logger = loggerFactory.CreateLogger<RssController>();
        }

        [HttpGet("{userSlug}/{podcastSlug}")]
        [HttpHead("{userSlug}/{podcastSlug}")]
        [Produces("application/xml")]
        [RssFeedAuthorize]
        public async Task<IActionResult> Get(string userSlug, string podcastSlug) {
            _logger.LogDebug($"RSS: Retrieving podcast: {userSlug} - {podcastSlug}");

            var user = await _userManager.FindBySlugAsync(userSlug);
            if (user is null) {
                //check if we have a redirect in place
                var redirect = await _redirectsRepository
                    .GetAll()
                    .Where(r => r.OldSlug == userSlug)
                    .FirstOrDefaultAsync();

                if (redirect is null) {
                    return NotFound();
                }
                user = await _userManager.FindByIdAsync(redirect.ApplicationUserId.ToString());
                if (user is null) {
                    return NotFound();
                }
                var url = $"{_appOptions.RssUrl}{user.Slug}/{podcastSlug}";
                return RedirectPermanent(url);
            }
            var podcast = await _podcastRepository.GetForUserAndSlugAsync(userSlug, podcastSlug);
            if (podcast is null) return NotFound();
            try {
                var xml = await ResourceReader.ReadResource("podcast.xml");
                var template = Handlebars.Compile(xml);
                var compiled = new PodcastEnclosureViewModel {
                    Title = podcast.Title,
                    Description = podcast.Description,
                    Author = "PodNoms Podcasts",
                    Image = podcast.GetImageUrl(_storageOptions.CdnUrl, _imageOptions.ContainerName)
                        .Replace("https://", "http://"),
                    Link = $"{_appOptions.RssUrl}{user.Slug}/{podcast.Slug}",
                    PublishDate = podcast.CreateDate.ToRFC822String(),
                    Category = podcast.Category?.Description,
                    Language = "en-IE",
                    Copyright = $"© {DateTime.Now.Year} PodNoms",
                    Owner = $"{user.FirstName} {user.LastName}",
                    OwnerEmail = user.Email,
                    ShowUrl = $"{_appOptions.SiteUrl}rss/{user.Slug}/{podcast.Slug}",

                    Items = (
                        from e in podcast.PodcastEntries
                        select new PodcastEnclosureItemViewModel {
                            Title = e.Title.StripNonXmlChars(),
                            Uid = e.Id.ToString(),
                            Description = e.Description.StripNonXmlChars(),
                            Author = e.Author.StripNonXmlChars(),
                            UpdateDate = e.CreateDate.ToRFC822String(),
                            AudioUrl = $"{_storageOptions.CdnUrl}{e.AudioUrl}".Replace("https://", "http://"),
                            AudioFileSize = e.AudioFileSize
                        }
                    ).ToList()
                };
                var result = template(compiled);
                return Content(result, "application/xml", Encoding.UTF8);
            } catch (NullReferenceException ex) {
                _logger.LogError(ex, "Error getting RSS", user, userSlug);
            }
            return NotFound();
        }
    }
}
