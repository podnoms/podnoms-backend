using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PodNoms.Common.Data.Settings;
using PodNoms.Common.Data.ViewModels.Resources;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Common.Utils;
using PodNoms.Data.Extensions;
using PodNoms.Data.Models;

namespace PodNoms.Api.Controllers {
    [Route("[controller]")]
    public class SearchController : BaseAuthController {
        private readonly StorageSettings _storageSettings;
        private readonly ImageFileStorageSettings _imageFileStorageSettings;
        private readonly IPodcastRepository _podcastRepository;
        private readonly IEntryRepository _entryRepository;

        public SearchController(
            IHttpContextAccessor contextAccessor,
             UserManager<ApplicationUser> userManager,
             ILogger<SearchController> logger,
             IOptions<StorageSettings> storageSettings,
             IOptions<ImageFileStorageSettings> imageFileStorageSettings,
             IPodcastRepository podcastRepository, IEntryRepository entryRepository) : base(contextAccessor, userManager, logger) {
            _storageSettings = storageSettings.Value;
            _imageFileStorageSettings = imageFileStorageSettings.Value;
            _podcastRepository = podcastRepository;
            _entryRepository = entryRepository;
        }

        [HttpGet("{query}")]
        public async Task<ActionResult<List<SearchResultsViewModel>>> DoSearch(string query) {
            var podcastResults = await _podcastRepository
                .GetAll()
                .Where(p => p.Title.Contains(query) || p.Description.Contains(query))
                .Select(p => new SearchResultsViewModel {
                    Title = p.Title,
                    Description = HtmlUtils.FormatLineBreaks(p.Description).Truncate(100),
                    ImageUrl = p.GetImageUrl(_storageSettings.CdnUrl, _imageFileStorageSettings.ContainerName),
                    Url = p.Slug,
                    Type = "Podcast"
                }).ToListAsync();

            var entryResults = await _entryRepository
                .GetAll()
                .Include(x => x.Podcast)
                .Where(p => p.Title.Contains(query) || p.Description.Contains(query))
                .Select(p => new SearchResultsViewModel {
                    Title = p.Title,
                    Description = HtmlUtils.FormatLineBreaks(p.Description).Truncate(100),
                    ImageUrl = p.GetImageUrl(_storageSettings.CdnUrl, _imageFileStorageSettings.ContainerName),
                    Url = p.Podcast.Slug,
                    Type = "Entry"
                }).ToListAsync();

            var mergedResults = podcastResults.Union(entryResults);
            return Ok(mergedResults);
        }
    }
}
