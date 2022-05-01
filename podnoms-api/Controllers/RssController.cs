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
using PodNoms.Common.Data;
using PodNoms.Common.Data.Settings;
using PodNoms.Common.Data.ViewModels.RssViewModels;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Common.Services.Caching;
using PodNoms.Common.Utils;
using PodNoms.Common.Utils.Extensions;
using PodNoms.Data.Enums;
using PodNoms.Data.Extensions;
using PodNoms.Data.Models;

namespace PodNoms.Api.Controllers {
    [Route("[controller]")]
    public class RssController : Controller {
        private readonly IPodcastRepository _podcastRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRepository<ApplicationUserSlugRedirects> _redirectsRepository;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly ILogger _logger;
        private readonly AppSettings _appSettings;
        private readonly StorageSettings _storageOptions;
        private readonly ImageFileStorageSettings _imageStorageOptions;
        private readonly AudioFileStorageSettings _audioStorageOptions;

        public RssController(IPodcastRepository podcastRespository,
            IOptions<AppSettings> appOptions,
            IOptions<ImageFileStorageSettings> imageStorageOptions,
            IOptions<AudioFileStorageSettings> audioStorageOptions,
            IOptions<StorageSettings> storageOptions,
            UserManager<ApplicationUser> userManager,
            IRepository<ApplicationUserSlugRedirects> redirectsRepository,
            IHttpContextAccessor contextAccessor,
            ILoggerFactory loggerFactory) {
            _podcastRepository = podcastRespository;
            _userManager = userManager;
            _redirectsRepository = redirectsRepository;
            _contextAccessor = contextAccessor;
            _appSettings = appOptions.Value;
            _imageStorageOptions = imageStorageOptions.Value;
            _audioStorageOptions = audioStorageOptions.Value;
            _storageOptions = storageOptions.Value;
            _logger = loggerFactory.CreateLogger<RssController>();
        }

        [HttpGet("{userSlug}/{podcastSlug}")]
        [HttpHead("{userSlug}/{podcastSlug}")]
        [Produces("application/xml")]
        [RssFeedAuthorize]
        [Cached("podcast", CacheType.Rss, "application/xml", 3600)]
        public async Task<IActionResult> Get(string userSlug, string podcastSlug) {
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

                var url = Flurl.Url.Combine(_appSettings.RssUrl, user.Slug, podcastSlug);
                return Redirect(url);
            }

            var podcast = await _podcastRepository.GetForUserAndSlugAsync(userSlug, podcastSlug);
            if (podcast is null) return NotFound();
            try {
                var xml = await ResourceReader.ReadResource("podcast.xml");
                var template = Handlebars.Compile(xml);
                var compiled = new PodcastEnclosureViewModel {
                    Title = podcast.Title,
                    Description = podcast.Description.RemoveUnwantedHtmlTags(),
                    Author = "PodNoms Podcasts",
                    Image = podcast.GetRawImageUrl(_storageOptions.CdnUrl, _imageStorageOptions.ContainerName),
                    Link = $"{_appSettings.PagesUrl}/{user.Slug}/{podcast.Slug}",
                    PublishDate = podcast.CreateDate.ToRFC822String(),
                    Category = podcast.Category?.Description,
                    Language = "en-IE",
                    Copyright = $"© {DateTime.Now.Year} PodNoms RSS",
                    Owner = $"{user.FirstName} {user.LastName}",
                    OwnerEmail = user.Email,
                    ShowUrl = Flurl.Url.Combine(_appSettings.RssUrl, user.Slug, podcast.Slug),
                    Items = (
                        from e in podcast.PodcastEntries
                        select new PodcastEnclosureItemViewModel {
                            Title = e.Title.StripNonXmlChars().RemoveUnwantedHtmlTags(),
                            Uid = e.Id.ToString(),
                            Summary = e.Description.StripNonXmlChars().RemoveUnwantedHtmlTags(),
                            Description = e.Description.StripNonXmlChars(),
                            Author = e.Author.StripNonXmlChars().Truncate(252, true),
                            EntryImage = e.GetImageUrl(_storageOptions.CdnUrl, _imageStorageOptions.ContainerName),
                            UpdateDate = e.CreateDate.ToRFC822String(),
                            AudioUrl = e.GetRssAudioUrl(_appSettings.AudioUrl),
                            AudioDuration = TimeSpan.FromSeconds(e.AudioLength).ToString(@"hh\:mm\:ss"),
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
