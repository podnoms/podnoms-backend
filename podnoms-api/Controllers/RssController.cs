using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HandlebarsDotNet;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
        private readonly ILogger _logger;
        private readonly AppSettings _appOptions;
        private readonly StorageSettings _storageOptions;
        private readonly ImageFileStorageSettings _imageOptions;

        public RssController(IPodcastRepository podcastRespository,
            IOptions<AppSettings> appOptions,
            IOptions<ImageFileStorageSettings> imageOptions,
            IOptions<StorageSettings> storageOptions,
            UserManager<ApplicationUser> userManager,
            ILoggerFactory loggerFactory) {
            _podcastRepository = podcastRespository;
            _userManager = userManager;
            _appOptions = appOptions.Value;
            _imageOptions = imageOptions.Value;
            _storageOptions = storageOptions.Value;
            _logger = loggerFactory.CreateLogger<RssController>();
        }

        [HttpGet("{slug}/{entry}")]
        [HttpHead("{slug}/{entry}")]
        [Produces("application/xml")]
        [RssFeedAuthorize]
        public async Task<IActionResult> Get(string slug, string entry) {
            _logger.LogDebug($"RSS: Retrieving podcast: {slug} - {entry}");

            var user = await _userManager.FindBySlugAsync(slug);
            if (user != null) {
                var podcast = await _podcastRepository.GetForUserAndSlugAsync(user.Id, entry);
                if (podcast == null) return NotFound();
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
                }
                catch (NullReferenceException ex) {
                    _logger.LogError(ex, "Error getting RSS", user, slug);
                }
            }
            else {
                _logger.LogError($"Unable to find user {slug}");
            }

            return NotFound();
        }
    }
}