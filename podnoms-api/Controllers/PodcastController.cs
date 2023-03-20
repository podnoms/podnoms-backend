using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PodNoms.Common.Data.Settings;
using PodNoms.Common.Data.Extensions;
using PodNoms.Common.Data.ViewModels.Resources;
using PodNoms.Common.Persistence;
using PodNoms.Common.Services.Storage;
using PodNoms.Data.Extensions;
using PodNoms.Data.Models;

namespace PodNoms.Api.Controllers {
    [Authorize]
    [Route("[controller]")]
    public class PodcastController : BaseAuthController {
        private readonly IMapper _mapper;
        private readonly IRepoAccessor _repo;
        private readonly StorageSettings _storageSettings;
        private readonly AppSettings _appSettings;
        private readonly ImageFileStorageSettings _fileStorageSettings;
        private readonly IFileUtilities _fileUtilities;

        public PodcastController(IMapper mapper, IRepoAccessor repo,
            ILogger<PodcastController> logger,
            UserManager<ApplicationUser> userManager,
            IHttpContextAccessor contextAccessor,
            IOptions<AppSettings> appSettings,
            IOptions<StorageSettings> storageSettings,
            IOptions<ImageFileStorageSettings> fileStorageSettings,
            IFileUtilities fileUtilities) : base(contextAccessor, userManager, logger) {
            _repo = repo;
            _storageSettings = storageSettings.Value;
            _appSettings = appSettings.Value;
            _fileStorageSettings = fileStorageSettings.Value;
            _fileUtilities = fileUtilities;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<List<PodcastViewModel>>> Get() {
            var podcasts = (await _repo.Podcasts
                .GetAllForUserAsync(_applicationUser.Id)).ToList();

            var ret = _mapper.Map<List<Podcast>, List<PodcastViewModel>>(podcasts);
            return Ok(ret);
        }

        [HttpGet("{slug}")]
        public async Task<IActionResult> GetBySlug(string slug) {
            var podcast = await _repo.Podcasts
                .GetForUserAndSlugAsync(Guid.Parse(_applicationUser.Id), slug);
            if (podcast is null)
                return NotFound();
            return Ok(_mapper.Map<Podcast, PodcastViewModel>(podcast));
        }

        [HttpGet("active")]
        public async Task<ActionResult<string>> GetActive() {
            var item = await _repo.Podcasts
                .GetActivePodcast(_applicationUser.Id);
            return this.Content($"\"{item}\"", "text/plain", Encoding.UTF8);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] PodcastViewModel vm) {
            if (!ModelState.IsValid) return BadRequest("Invalid podcast model");

            var item = _mapper.Map<PodcastViewModel, Podcast>(vm);

            var isNew = string.IsNullOrEmpty(vm.Id);
            item.AppUser = _applicationUser;
            var ret = await _repo.Podcasts
                .AddOrUpdate(item);
            try {
                await _repo.CompleteAsync();

                if (!isNew) return Ok(_mapper.Map<Podcast, PodcastViewModel>(ret));

                // TODO: Revisit this at some stage, horribly hacky & brittle
                // TODO: This should be moved to the background cache images job
                // TODO: And ultimately handled by imageresizer when they get their fucking docs in order
                var rawImageFileName = vm.ImageUrl?.Replace(_storageSettings.CdnUrl, string.Empty).TrimStart('/');
                if (string.IsNullOrEmpty(rawImageFileName)) return Ok(_mapper.Map<Podcast, PodcastViewModel>(ret));

                var parts = rawImageFileName.Split('/', 2);
                if (parts.Length != 2) return Ok(_mapper.Map<Podcast, PodcastViewModel>(ret));

                await _fileUtilities.CopyRemoteFile(
                    parts[0], parts[1],
                    _fileStorageSettings.ContainerName, $"podcast/{ret.Id.ToString()}.png");

                return Ok(_mapper.Map<Podcast, PodcastViewModel>(ret));
            } catch (GenerateSlugFailureException) {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] PodcastViewModel vm) {
            if (!ModelState.IsValid || string.IsNullOrEmpty(vm.Id)) return BadRequest("Invalid request data");

            var podcast = _mapper.Map<PodcastViewModel, Podcast>(vm);
            podcast.AppUser ??= _applicationUser;

            _repo.Podcasts
                .AddOrUpdate(podcast);
            await _repo.CompleteAsync();
            return Ok(_mapper.Map<Podcast, PodcastViewModel>(podcast));
        }

        // TODO: This needs to be wrapped in an authorize - group=test-harness ASAP
        [HttpDelete]
        [Authorize(Roles = "catastrophic-api-calls-allowed")]
        public async Task<IActionResult> DeleteAllPodcasts() {
            try {
                _repo.Context.Podcasts.RemoveRange(
                    _repo.Context.Podcasts.ToList()
                );
                await _repo.CompleteAsync();
                return Ok();
            } catch (Exception ex) {
                _logger.LogError(564378, ex, "Error deleting podcasts");
            }

            return BadRequest("Unable to delete podcasts");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id) {
            try {
                await _repo.Podcasts
                    .DeleteAsync(new Guid(id));
                await _repo.CompleteAsync();
                return Ok();
            } catch (Exception ex) {
                _logger.LogError(54738, ex, "Error deleting podcast");
            }

            return BadRequest("Unable to delete entry");
        }

        [HttpGet("checkslug/{slug}")]
        public async Task<ActionResult<string>> CheckSlug(string slug) {
            var slugValid = (await _repo.Podcasts
                    .GetAllForUserAsync(_applicationUser.Id))
                .Where(r => r.Slug == slug);

            var enumerable = slugValid as Podcast[] ?? slugValid.ToArray();
            var content = !enumerable.Any() ? string.Empty : enumerable.First().Title;

            return base.Content(
                $"\"{content}\"",
                "text/plain"
            );
        }

        [HttpGet("opml")]
        [Produces("application/xml")]
        public async Task<ActionResult<string>> GetOpml() {
            var result = await _applicationUser.GetOpmlFeed(
                _repo,
                _appSettings.RssUrl,
                _appSettings.SiteUrl);
            return Content(result, "application/xml", Encoding.UTF8);
        }
    }
}
