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
using PodNoms.Common.Persistence;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Common.Utils;
using PodNoms.Data.Extensions;
using PodNoms.Data.Models;

namespace PodNoms.Api.Controllers {
    [Route("[controller]")]
    public class SearchController : BaseAuthController {
        private readonly IRepoAccessor _repo;
        private readonly StorageSettings _storageSettings;
        private readonly ImageFileStorageSettings _imageFileStorageSettings;

        public SearchController(
            IHttpContextAccessor contextAccessor,
            UserManager<ApplicationUser> userManager,
            ILogger logger,
            IOptions<StorageSettings> storageSettings,
            IOptions<ImageFileStorageSettings> imageFileStorageSettings,
            IRepoAccessor repo
        ) : base(contextAccessor, userManager, logger) {
            _repo = repo;
            _storageSettings = storageSettings.Value;
            _imageFileStorageSettings = imageFileStorageSettings.Value;
        }

        [HttpGet("{query}")]
        public async Task<ActionResult<List<SearchResultsViewModel>>> DoSearch(string query) {
            var podcastResults = await _repo.Podcasts
                .GetAll()
                .Where(p => p.AppUser.Id == _applicationUser.Id)
                .Where(p => p.Title.Contains(query) || p.Description.Contains(query))
                .Select(p => new SearchResultsViewModel {
                    Title = p.Title,
                    Description = HtmlUtils.FormatLineBreaks(p.Description).Truncate(100, true),
                    ImageUrl = p.GetImageUrl(_storageSettings.CdnUrl, _imageFileStorageSettings.ContainerName),
                    Url = p.Slug,
                    Type = "Podcast",
                    DateCreated = p.CreateDate
                }).ToListAsync();

            var entryResults = await _repo.Entries
                .GetAll()
                .Include(x => x.Podcast)
                .Where(p => p.Podcast.AppUser.Id == _applicationUser.Id)
                .Where(p => p.Title.Contains(query) || p.Description.Contains(query))
                .Select(p => new SearchResultsViewModel {
                    Title = p.Title,
                    Description = HtmlUtils.FormatLineBreaks(p.Description).Truncate(100, true),
                    ImageUrl = p.GetImageUrl(_storageSettings.CdnUrl, _imageFileStorageSettings.ContainerName),
                    Url = p.Podcast.Slug,
                    Type = "Entry",
                    DateCreated = p.CreateDate
                }).ToListAsync();

            var mergedResults = podcastResults.Union(entryResults);
            return Ok(mergedResults);
        }
    }
}
