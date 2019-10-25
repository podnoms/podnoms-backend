using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PodNoms.Common.Data.Settings;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Common.Services.Storage;
using PodNoms.Data.Models;

namespace PodNoms.Api.Controllers {
    [Route("[controller]")]
    [Authorize]
    public class DownloadController : BaseAuthController {
        private readonly StorageSettings _storageSettings;
        private readonly AudioFileStorageSettings _audioStorageSettings;
        private readonly IFileUtilities _fileUtilities;
        private readonly IEntryRepository _repository;

        public DownloadController(IHttpContextAccessor contextAccessor,
                                  IOptions<StorageSettings> storageSettings,
                                  IOptions<AudioFileStorageSettings> audioStorageSettings,
                                  IFileUtilities fileUtilities,
                                  UserManager<ApplicationUser> userManager,
                                  ILogger<DownloadController> logger,
                                  IEntryRepository repository) :
                base(contextAccessor, userManager, logger) {
            _storageSettings = storageSettings.Value;
            _audioStorageSettings = audioStorageSettings.Value;
            _fileUtilities = fileUtilities;
            _repository = repository;
        }
        [AllowAnonymous]
        [HttpGet("{entryId}")]
        public async Task<IActionResult> DownloadFile(string entryId) {
            try {
                var entry = await _repository.GetAsync(entryId);
                var storageUrl = entry.GetInternalStorageUrl(_storageSettings.CdnUrl);
                var stream = await _fileUtilities.GetRemoteFileStream(_audioStorageSettings.ContainerName, $"{entry.Id}.mp3");
                Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{entry.GetFileDownloadName()}\"");
                Response.Headers.Add("Content-Type", $"application/octet-stream");
                return File(stream, "application/octet-stream", false);
            } catch (InvalidOperationException e) {
                _logger.LogError(e.Message);
                return NotFound();
            }
        }
    }
}
