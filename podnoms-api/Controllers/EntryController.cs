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
        private readonly IPodcastRepository _podcastRepository;
        private readonly IEntryRepository _repository;

        private IConfiguration _options;

        private readonly IUnitOfWork _unitOfWork;
        private readonly IResponseCacheService _cache;
        private readonly IMapper _mapper;
        private readonly IYouTubeParser _youTubeParser;
        private readonly AppSettings _appSettings;
        private readonly IUrlProcessService _processor;
        private readonly EntryPreProcessor _preProcessor;
        private readonly AudioFileStorageSettings _audioFileStorageSettings;
        private readonly StorageSettings _storageSettings;

        public EntryController(IEntryRepository repository,
            IPodcastRepository podcastRepository,
            IUnitOfWork unitOfWork, IMapper mapper,
            IOptions<StorageSettings> storageSettings,
            IOptions<AppSettings> appSettings,
            IOptions<AudioFileStorageSettings> audioFileStorageSettings,
            IYouTubeParser youTubeParser,
            IConfiguration options,
            IResponseCacheService cache,
            IUrlProcessService processor,
            EntryPreProcessor preProcessor,
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
            _cache = cache;
            _youTubeParser = youTubeParser;
            _processor = processor;
            _preProcessor = preProcessor;
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
        [Authorize(AuthenticationSchemes = "Bearer, PodNomsApiKey")]
        public async Task<ActionResult<PodcastEntryViewModel>> Post([FromBody] PodcastEntryViewModel item) {
            if (!ModelState.IsValid)
                return BadRequest("Invalid podcast entry posted");
            PodcastEntry entry;

            if (item.Id != null) {
                entry = await _repository.GetAsync(item.Id);
                _mapper.Map<PodcastEntryViewModel, PodcastEntry>(item, entry);
            } else {
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
            //TODO: This should return the properties bundle
            //with the status as a member
            var status = await _processor.GetInformation(entry);
            if (status != RemoteUrlType.Invalid) {
                // check user quota
                var result = await _preProcessor.PreProcessEntry(
                    _applicationUser,
                    entry);
                if (result == EntryProcessResult.QuotaExceeded) {
                    return StatusCode(402);
                } else if (result == EntryProcessResult.GeneralFailure) {
                    return BadRequest();
                }

                _repository.GetContext()
                    .Entry(entry)
                    .State = EntityState.Detached;

                entry = await _repository.GetAsync(entry.Id);
                return _mapper.Map<PodcastEntry, PodcastEntryViewModel>(entry);

            } else if ((status == RemoteUrlType.Playlist && _youTubeParser.ValidateUrl(item.SourceUrl)) ||
                       MixcloudParser.ValidateUrl(item.SourceUrl)) {
                entry.ProcessingStatus = ProcessingStatus.Deferred;
                var result = _mapper.Map<PodcastEntry, PodcastEntryViewModel>(entry);
                return Accepted(result);
            }

            return BadRequest("Failed to create podcast entry");
        }

        // TODO: This needs to be wrapped in an authorize - group=test-harness ASAP
        [HttpDelete]
        public async Task<IActionResult> DeleteAllEntries() {
            try {
                _repository.GetContext().PodcastEntries.RemoveRange(
                    _repository.GetContext().PodcastEntries.ToList()
                );
                await _unitOfWork.CompleteAsync();
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
                var entry = await _repository.GetAsync(id);
                await _repository.DeleteAsync(id);
                await _unitOfWork.CompleteAsync();
                return Ok();
            } catch (Exception ex) {
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
                BackgroundJob.Enqueue<ProcessNewEntryJob>(e =>
                    e.ProcessEntry(entry.Id, null));
            }

            return Ok(entry);
        }

        [HttpGet("downloadurl/{entryId}")]
        public async Task<ActionResult<AudioDownloadInfoViewModel>> GetDownloadUrl(string entryId) {
            var entry = await _repository.GetAsync(entryId);

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
