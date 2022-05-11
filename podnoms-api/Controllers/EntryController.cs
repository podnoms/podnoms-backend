using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PodNoms.AudioParsing.UrlParsers;
using PodNoms.Common.Data.Settings;
using PodNoms.Common.Data.ViewModels;
using PodNoms.Common.Data.ViewModels.Resources;
using PodNoms.Common.Persistence;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Common.Services.Caching;
using PodNoms.Common.Services.Jobs;
using PodNoms.Common.Services.Processor;
using PodNoms.Common.Utils.RemoteParsers;
using PodNoms.Data.Enums;
using PodNoms.Data.Models;
using static PodNoms.Common.Services.Processor.EntryPreProcessor;

namespace PodNoms.Api.Controllers {
    [Route("[controller]")]
    [Authorize]
    public class EntryController : BaseAuthController {
        private readonly IRepoAccessor _repo;
        private readonly IMapper _mapper;
        private readonly IUrlProcessService _processor;
        private readonly EntryPreProcessor _preProcessor;
        private readonly StorageSettings _storageSettings;

        public EntryController(IRepoAccessor repo, IMapper mapper,
            IOptions<StorageSettings> storageSettings,
            IUrlProcessService processor,
            EntryPreProcessor preProcessor,
            ILogger logger,
            UserManager<ApplicationUser> userManager,
            IHttpContextAccessor contextAccessor) : base(contextAccessor, userManager, logger) {
            _storageSettings = storageSettings.Value;
            _repo = repo;
            _mapper = mapper;
            _processor = processor;
            _preProcessor = preProcessor;
        }

        [HttpGet]
        public async Task<ActionResult<List<PodcastEntryViewModel>>> GetAllForUser() {
            var entries = await _repo.Entries.GetAllForUserAsync(_applicationUser.Id);
            var results = _mapper.Map<List<PodcastEntry>, List<PodcastEntryViewModel>>(
                entries.OrderByDescending(e => e.CreateDate).ToList()
            );
            return Ok(results);
        }

        [HttpGet("all/{podcastSlug}")]
        public async Task<ActionResult<List<PodcastEntryViewModel>>> GetAllForSlug(string podcastSlug) {
            var entries = await _repo.Entries.GetAllForSlugAsync(podcastSlug);
            var results = _mapper.Map<List<PodcastEntry>, List<PodcastEntryViewModel>>(
                entries.OrderByDescending(r => r.CreateDate).ToList()
            );

            return Ok(results);
        }

        [HttpGet("{entryId}")]
        public async Task<ActionResult<PodcastEntryViewModel>> Get(string entryId) {
            var entry = await _repo.Entries.GetAll()
                .Include(e => e.Podcast)
                .SingleOrDefaultAsync(e => e.Id == Guid.Parse(entryId));
            var result = _mapper.Map<PodcastEntry, PodcastEntryViewModel>(entry);
            return Ok(result);
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = "Bearer, PodNomsApiKey")]
        public async Task<ActionResult<PodcastEntryViewModel>> Post([FromBody] PodcastEntryViewModel item) {
            if (!ModelState.IsValid)
                return BadRequest("Invalid podcast entry posted");
            PodcastEntry entry;

            if (item.Id != null) {
                entry = await _repo.Entries.GetAsync(item.Id);
                _mapper.Map(item, entry);
            } else {
                entry = _mapper.Map<PodcastEntryViewModel, PodcastEntry>(item);
            }

            if (entry is null)
                return BadRequest();

            if (entry.ProcessingStatus is ProcessingStatus.Uploading or ProcessingStatus.Processed) {
                // we're editing an existing entry
                _repo.Entries.AddOrUpdate(entry);
                await _repo.CompleteAsync();
                var result = _mapper.Map<PodcastEntry, PodcastEntryViewModel>(entry);
                return Ok(result);
            }

            //we're adding a new entry
            //TODO: This should return the properties bundle
            //with the status as a member
            var parser = new UrlTypeParser();
            var urlType = await parser.GetUrlType(entry.SourceUrl);

            if (!urlType.Equals(UrlType.Invalid) &&
                !urlType.Equals(UrlType.Playlist) &&
                !urlType.Equals(UrlType.Channel)) {
                // check user quota
                var result = await _preProcessor.PreProcessEntry(
                    _applicationUser,
                    entry);

                switch (result) {
                    case EntryProcessResult.QuotaExceeded:
                        return StatusCode(402);
                    case EntryProcessResult.GeneralFailure:
                        return BadRequest();
                    case EntryProcessResult.Succeeded:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                _repo.Context
                    .Entry(entry)
                    .State = EntityState.Detached;

                entry = await _repo.Entries.GetAsync(entry.Id);
                return _mapper.Map<PodcastEntry, PodcastEntryViewModel>(entry);
            }

            if (!urlType.Equals(UrlType.Playlist)) {
                return BadRequest("Failed to create podcast entry");
            }

            {
                entry.ProcessingStatus = ProcessingStatus.Deferred;
                var result = _mapper.Map<PodcastEntry, PodcastEntryViewModel>(entry);
                return Accepted(result);
            }
        }

        // TODO: This needs to be wrapped in an authorize - group=test-harness ASAP
        [HttpDelete]
        public async Task<IActionResult> DeleteAllEntries() {
            try {
                _repo.Context.PodcastEntries.RemoveRange(
                    _repo.Context.PodcastEntries.ToList()
                );
                await _repo.CompleteAsync();
                return Ok();
            } catch (Exception ex) {
                _logger.LogError("Error deleting entries");
                _logger.LogError(ex.Message);
            }

            return BadRequest("Unable to delete entries");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id) {
            try {
                await _repo.Entries.DeleteAsync(id);
                await _repo.CompleteAsync();
                return Ok();
            } catch (Exception ex) {
                _logger.LogError("Error deleting entry");
                _logger.LogError(ex.Message);
            }

            return BadRequest("Unable to delete entry");
        }

        [HttpPost("/preprocess")]
        public async Task<ActionResult<PodcastEntryViewModel>> PreProcess(PodcastEntryViewModel item) {
            var entry = await _repo.Entries.GetAsync(item.Id);
            entry.ProcessingStatus = ProcessingStatus.Accepted;
            var response = _processor.GetInformation(item.Id, this.UserId);
            entry.ProcessingStatus = ProcessingStatus.Processing;
            await _repo.CompleteAsync();

            var result = _mapper.Map<PodcastEntry, PodcastEntryViewModel>(entry);
            return result;
        }

        [HttpPost("resubmit")]
        public async Task<IActionResult> ReSubmit([FromBody] PodcastEntryViewModel item) {
            var entry = await _repo.Entries.GetAsync(item.Id);
            entry.ProcessingStatus = ProcessingStatus.Processing;
            await _repo.CompleteAsync();
            if (entry.ProcessingStatus != ProcessingStatus.Processed) {
                BackgroundJob.Enqueue<ProcessNewEntryJob>(e =>
                    e.ProcessEntry(entry.Id, null));
            }

            return Ok(entry);
        }

        [HttpGet("downloadurl/{entryId}")]
        public async Task<ActionResult<AudioDownloadInfoViewModel>> GetDownloadUrl(string entryId) {
            var entry = await _repo.Entries.GetAsync(entryId);

            if (entry is null) {
                return NotFound();
            }

            return Ok(new AudioDownloadInfoViewModel {
                Url = entry.GetInternalStorageUrl(_storageSettings.CdnUrl),
                Filename = $"{entry.Title}.mp3"
            });
        }
    }
}
