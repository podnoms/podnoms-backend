using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PodNoms.Common;
using PodNoms.Common.Auth;
using PodNoms.Common.Data.Settings;
using PodNoms.Common.Data.ViewModels.Resources;
using PodNoms.Common.Persistence;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Common.Services;
using PodNoms.Common.Services.Jobs;
using PodNoms.Common.Services.Processor;
using PodNoms.Common.Utils.RemoteParsers;
using PodNoms.Data.Models;

namespace PodNoms.Api.Controllers {
    [Route("[controller]")]
    [Authorize]
    public class EntryController : BaseAuthController {
        private readonly IPodcastRepository _podcastRepository;
        private readonly IEntryRepository _repository;

        private IConfiguration _options;

        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly AppSettings _appSettings;
        private readonly IUrlProcessService _processor;
        private readonly AudioFileStorageSettings _audioFileStorageSettings;
        private readonly StorageSettings _storageSettings;

        public EntryController(IEntryRepository repository,
            IPodcastRepository podcastRepository,
            IUnitOfWork unitOfWork, IMapper mapper, IOptions<StorageSettings> storageSettings,
            IOptions<AppSettings> appSettings,
            IOptions<AudioFileStorageSettings> audioFileStorageSettings,
            IConfiguration options,
            IUrlProcessService processor,
            ILogger<EntryController> logger,
            UserManager<ApplicationUser> userManager,
            IHttpContextAccessor contextAccessor) : base(contextAccessor, userManager, logger) {
            _podcastRepository = podcastRepository;
            _repository = repository;
            _options = options;
            _appSettings = appSettings.Value;
            _storageSettings = storageSettings.Value;
            _unitOfWork = unitOfWork;
            _audioFileStorageSettings = audioFileStorageSettings.Value;
            _mapper = mapper;
            _processor = processor;
        }

        [HttpGet]
        public async Task<ActionResult<List<PodcastEntryViewModel>>> GetAllForUser() {
            var entries = await _repository.GetAllForUserAsync(_applicationUser.Id);
            var results = _mapper.Map<List<PodcastEntry>, List<PodcastEntryViewModel>>(
                entries.OrderByDescending(e => e.CreateDate).ToList()
            );
            return Ok(results);
        }

        [HttpGet("all/{podcastSlug}")]
        public async Task<ActionResult<List<PodcastEntryViewModel>>> GetAllForSlug(string podcastSlug) {
            var entries = await _repository.GetAllForSlugAsync(podcastSlug);
            var results = _mapper.Map<List<PodcastEntry>, List<PodcastEntryViewModel>>(
                entries.OrderByDescending(r => r.CreateDate).ToList()
            );

            return Ok(results);
        }

        [HttpGet("{entryId}")]
        public async Task<ActionResult<PodcastEntryViewModel>> Get(string entryId) {
            var entry = await _repository.GetAll()
                .Include(e => e.Podcast)
                .SingleOrDefaultAsync(e => e.Id == Guid.Parse(entryId));
            var result = _mapper.Map<PodcastEntry, PodcastEntryViewModel>(entry);
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<PodcastEntryViewModel>> Post([FromBody] PodcastEntryViewModel item) {
            if (!ModelState.IsValid)
                return BadRequest("Invalid podcast entry posted");
            PodcastEntry entry;

            if (item.Id != null) {
                entry = await _repository.GetAsync(item.Id);
                _mapper.Map<PodcastEntryViewModel, PodcastEntry>(item, entry);
            }
            else {
                entry = _mapper.Map<PodcastEntryViewModel, PodcastEntry>(item);
            }

            if (entry is null)
                return BadRequest();

            if (entry.ProcessingStatus == ProcessingStatus.Uploading ||
                entry.ProcessingStatus == ProcessingStatus.Processed) {
                // we're editing an existing entry
                _repository.AddOrUpdate(entry);
                await _unitOfWork.CompleteAsync();
                var result = _mapper.Map<PodcastEntry, PodcastEntryViewModel>(entry);
                return Ok(result);
            }

            //we're adding a new entry
            var status = await _processor.GetInformation(entry);
            if (status == AudioType.Valid) {
                // check user quota
                var quota = _applicationUser.DiskQuota ?? _storageSettings.DefaultUserQuota;
                var totalUsed = (await _repository.GetAllForUserAsync(_applicationUser.Id))
                    .Select(x => x.AudioFileSize)
                    .Sum();
                if (totalUsed >= quota) {
                    return StatusCode(402);
                }

                if (entry.ProcessingStatus != ProcessingStatus.Processing)
                    return BadRequest("Failed to create podcast entry");

                if (string.IsNullOrEmpty(entry.ImageUrl)) {
                    entry.ImageUrl = $"{_storageSettings.CdnUrl}static/images/default-entry.png";
                }

                entry.Processed = false;
                _repository.AddOrUpdate(entry);
                try {
                    var succeeded = await _unitOfWork.CompleteAsync();
                    await _repository.LoadPodcastAsync(entry);
                    if (succeeded) {
                        BackgroundJob.Enqueue<ProcessNewEntryJob>(e => e.ProcessEntry(entry.Id));
                        return _mapper.Map<PodcastEntry, PodcastEntryViewModel>(entry);
                    }
                }
                catch (DbUpdateException e) {
                    _logger.LogError(e.Message);
                    return BadRequest(item);
                }
            }
            else if ((status == AudioType.Playlist && YouTubeParser.ValidateUrl(item.SourceUrl)) ||
                     MixcloudParser.ValidateUrl(item.SourceUrl)) {
                entry.ProcessingStatus = ProcessingStatus.Deferred;
                var result = _mapper.Map<PodcastEntry, PodcastEntryViewModel>(entry);
                return Accepted(result);
            }

            return BadRequest("Failed to create podcast entry");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id) {
            try {
                await _repository.DeleteAsync(new Guid(id));
                await _unitOfWork.CompleteAsync();
                return Ok();
            }
            catch (Exception ex) {
                _logger.LogError("Error deleting entry");
                _logger.LogError(ex.Message);
            }

            return BadRequest("Unable to delete entry");
        }

        [HttpPost("/preprocess")]
        public async Task<ActionResult<PodcastEntryViewModel>> PreProcess(PodcastEntryViewModel item) {
            var entry = await _repository.GetAsync(item.Id);
            entry.ProcessingStatus = ProcessingStatus.Accepted;
            var response = _processor.GetInformation(item.Id);
            entry.ProcessingStatus = ProcessingStatus.Processing;
            await _unitOfWork.CompleteAsync();

            var result = _mapper.Map<PodcastEntry, PodcastEntryViewModel>(entry);
            return result;
        }

        [HttpPost("resubmit")]
        public async Task<IActionResult> ReSubmit([FromBody] PodcastEntryViewModel item) {
            var entry = await _repository.GetAsync(item.Id);
            entry.ProcessingStatus = ProcessingStatus.Processing;
            await _unitOfWork.CompleteAsync();
            if (entry.ProcessingStatus != ProcessingStatus.Processed) {
                BackgroundJob.Enqueue<ProcessNewEntryJob>(e => e.ProcessEntry(entry.Id));
            }

            return Ok(entry);
        }

        [HttpGet("downloadurl/{entryId}")]
        public async Task<ActionResult<string>> GetDownloadUrl(string entryId) {
            var entry = await _repository.GetAsync(entryId);

            if (entry is null) {
                return NotFound();
            }

            return Ok(new {
                url = $"{_storageSettings.CdnUrl}{entry.AudioUrl}",
                title = entry.Title
            });
        }
    }
}