using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PodNoms.Data.Models;
using PodNoms.Common.Auth;
using PodNoms.Common.Data.Settings;
using PodNoms.Common.Data.ViewModels.Resources;
using PodNoms.Common.Persistence;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Common.Services.Storage;
using PodNoms.Data.Extensions;

namespace PodNoms.Api.Controllers {
    [Authorize]
    [Route("[controller]")]
    public class PodcastController : BaseAuthController {
        private readonly IPodcastRepository _repository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly StorageSettings _storageSettings;
        private readonly ImageFileStorageSettings _fileStorageSettings;
        private readonly IFileUtilities _fileUtilities;

        public PodcastController(IPodcastRepository repository, IMapper mapper, IUnitOfWork unitOfWork,
            ILogger<PodcastController> logger,
            UserManager<ApplicationUser> userManager, IHttpContextAccessor contextAccessor,
            IOptions<StorageSettings> storageSettings, IOptions<ImageFileStorageSettings> fileStorageSettings,
            IFileUtilities fileUtilities)
            : base(contextAccessor, userManager, logger) {
            _uow = unitOfWork;
            _storageSettings = storageSettings.Value;
            _fileStorageSettings = fileStorageSettings.Value;
            _fileUtilities = fileUtilities;
            _repository = repository;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<PodcastViewModel>> Get() {
            var podcasts = await _repository.GetAllForUserAsync(_applicationUser.Id);
            var ret = _mapper.Map<List<Podcast>, List<PodcastViewModel>>(podcasts.ToList());
            return Ok(ret);
        }

        [HttpGet("{slug}")]
        public async Task<IActionResult> GetBySlug(string slug) {
            var podcast = await _repository.GetAsync(_applicationUser.Id, slug);
            if (podcast == null)
                return NotFound();
            return Ok(_mapper.Map<Podcast, PodcastViewModel>(podcast));
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] PodcastViewModel vm) {
            if (!ModelState.IsValid) return BadRequest("Invalid podcast model");

            var item = _mapper.Map<PodcastViewModel, Podcast>(vm);

            var isNew = string.IsNullOrEmpty(vm.Id);
            item.AppUser = _applicationUser;
            var ret = _repository.AddOrUpdate(item);
            try {
                await _uow.CompleteAsync();

                if (!isNew) return Ok(_mapper.Map<Podcast, PodcastViewModel>(ret));

                // TODO: Revisit this at some stage, horribly hacky & brittle
                var rawImageFileName = vm.ImageUrl?.Replace(_storageSettings.CdnUrl, string.Empty).TrimStart('/');
                if (string.IsNullOrEmpty(rawImageFileName)) return Ok(_mapper.Map<Podcast, PodcastViewModel>(ret));

                var parts = rawImageFileName.Split('/', 2);
                if (parts.Length != 2) return Ok(_mapper.Map<Podcast, PodcastViewModel>(ret));

                await _fileUtilities.CopyRemoteFile(
                    parts[0], parts[1],
                    _fileStorageSettings.ContainerName, $"podcast/{ret.Id.ToString()}.png");

                await _fileUtilities.CopyRemoteFile(
                    parts[0], parts[1].Replace(".png", "-32x32.png"),
                    _fileStorageSettings.ContainerName, $"podcast/{ret.Id.ToString()}-32x32.png");

                return Ok(_mapper.Map<Podcast, PodcastViewModel>(ret));
            }
            catch (GenerateSlugFailureException e) {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] PodcastViewModel vm) {
            if (ModelState.IsValid && !string.IsNullOrEmpty(vm.Id)) {
                var podcast = _mapper.Map<PodcastViewModel, Podcast>(vm);
                if (podcast.AppUser is null)
                    podcast.AppUser = _applicationUser;

                _repository.AddOrUpdate(podcast);
                await _uow.CompleteAsync();
                return Ok(_mapper.Map<Podcast, PodcastViewModel>(podcast));
            }

            return BadRequest("Invalid request data");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id) {
            try {
                await _repository.DeleteAsync(new Guid(id));
                await _uow.CompleteAsync();
                return Ok();
            }
            catch (Exception ex) {
                _logger.LogError("Error deleting podcast");
                _logger.LogError(ex.Message);
            }

            return BadRequest("Unable to delete entry");
        }

        [HttpGet("checkslug/{slug}")]
        public async Task<ActionResult<string>> CheckSlug(string slug) {
            var slugValid = (await _repository.GetAllForUserAsync(_applicationUser.Id))
                .Where(r => r.Slug == slug);
            var content = slugValid.Count() == 0 ? string.Empty : slugValid.First().Title;
            return base.Content(
                $"\"{content}\"",
                "text/plain"
            );
        }
    }
}