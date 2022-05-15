using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PodNoms.Common.Auth;
using PodNoms.Common.Data.Settings;
using PodNoms.Common.Data.ViewModels.Resources;
using PodNoms.Common.Persistence;
using PodNoms.Data.Models;
using PodNoms.Data.Utils;

namespace PodNoms.Api.Controllers {
    [Authorize]
    [Route("[controller]")]
    [EnableCors("DefaultCors")]
    public class ProfileController : BaseAuthController {

        private readonly AppSettings _appSettings;
        private readonly ApiKeyAuthSettings _apiKeyAuthSettings;
        private readonly StorageSettings _storageSettings;
        private readonly IMapper _mapper;
        private readonly IRepoAccessor _repo;

        public ProfileController(IMapper mapper, IRepoAccessor repo, ILogger<ProfileController> logger,
            IOptions<AppSettings> appSettings,
            IOptions<ApiKeyAuthSettings> apiKeyAuthSettings,
            IOptions<StorageSettings> storageSettings,
            UserManager<ApplicationUser> userManager, IHttpContextAccessor contextAccessor) : base(contextAccessor,
            userManager, logger) {
            _appSettings = appSettings.Value;
            _apiKeyAuthSettings = apiKeyAuthSettings.Value;
            _mapper = mapper;
            _storageSettings = storageSettings.Value;
            _repo = repo;
        }

        //TODO: This shouldn't be a List?
        [HttpGet]
        public ActionResult<List<ProfileViewModel>> Get() {
            var result = _mapper.Map<ApplicationUser, ProfileViewModel>(_applicationUser);
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<ProfileViewModel>> Post([FromBody] ProfileViewModel item) {
            _logger.LogInformation($"Getting profile: {item.Id}");
            //TODO: Create a mapping for this.
            if (!string.IsNullOrEmpty(_applicationUser.Slug) && !_applicationUser.Slug.Equals(item.Slug)) {
                //item has changed, store the old slug for redirect purposes
                var slugRedirectsRepo = _repo.CreateProxy<ApplicationUserSlugRedirects>();
                var existing = await slugRedirectsRepo
                    .GetAll()
                    .Where(r => r.ApplicationUser.Id == _applicationUser.Id && r.OldSlug == item.Slug)
                    .FirstOrDefaultAsync();
                if (existing is null) {
                    slugRedirectsRepo.AddOrUpdate(new ApplicationUserSlugRedirects {
                        ApplicationUser = _applicationUser,
                        OldSlug = _applicationUser.Slug
                    });
                    await _repo.CompleteAsync();
                }
            }

            _applicationUser.Slug = item.Slug;
            _applicationUser.FirstName = item.FirstName;
            _applicationUser.LastName = item.LastName;
            _applicationUser.TwitterHandle = item.TwitterHandle;
            _applicationUser.EmailNotificationOptions = (NotificationOptions)item.EmailNotificationOptions;
            await _userManager.UpdateAsync(_applicationUser);
            var ret = _mapper.Map<ApplicationUser, ProfileViewModel>(_applicationUser);
            return Ok(ret);
        }

        [AllowAnonymous]
        [HttpGet("checkemail/{email}")]
        public async Task<ActionResult<bool>> CheckEmail(string email) {
            var emailValid = await _userManager.CheckEmail(email) ||
                             (_applicationUser != null && (email.ToLower().Equals(_applicationUser.Email.ToLower())));
            return Ok(emailValid);
        }

        [AllowAnonymous]
        [HttpGet("checkslug/{slug}")]
        public async Task<ActionResult<bool>> CheckSlug(string slug) {
            var slugValid = await _userManager.CheckSlug(slug) ||
                            (_applicationUser != null && (slug.Equals(_applicationUser.Slug)));
            return Ok(slugValid);
        }

        [HttpGet("needsredirect")]
        public IActionResult NeedsRedirect() {
            if (_applicationUser.Slug.Contains("podnoms-user")) {
                return Ok();
            }

            return NoContent();
        }

        [HttpGet("getkeys")]
        public async Task<ActionResult<ApiKeyViewModel[]>> GetApiKeys() {
            var issuedApiKeyRepository = _repo.CreateProxy<IssuedApiKey>();
            var keys = await issuedApiKeyRepository.GetAll()
                .Where(r => r.IssuedTo.Slug.Equals(_applicationUser.Slug))
                .OrderByDescending(r => r.CreateDate)
                .ToListAsync();
            return _mapper.Map<ApiKeyViewModel[]>(keys);
        }

        [HttpPost("regeneratekey")]
        public async Task<ActionResult<ApiKeyViewModel>> RegenerateApiKey([FromBody] ApiKeyViewModel apiKeyRequest) {
            var issuedApiKeyRepository = _repo.CreateProxy<IssuedApiKey>();
            var existingKey = await issuedApiKeyRepository.GetAsync(apiKeyRequest.Id);
            if (existingKey is null) {
                return NotFound();
            }

            var newKey = await _generateApiKey(apiKeyRequest);
            if (newKey == null) {
                return BadRequest();
            }

            newKey.Name = existingKey.Name;
            newKey.DateIssued = DateTime.Today;
            await issuedApiKeyRepository.DeleteAsync(existingKey.Id);
            await _repo.CompleteAsync();

            return newKey;
        }

        [HttpPost("requestkey")]
        public async Task<ActionResult<ApiKeyViewModel>> RequestApiKey([FromBody] ApiKeyViewModel apiKeyRequest) {
            if (!ModelState.IsValid) return BadRequest("Invalid api key model");
            return await _generateApiKey(apiKeyRequest);
        }

        private async Task<ApiKeyViewModel> _generateApiKey(ApiKeyViewModel apiKeyRequest) {
            var issuedApiKeyRepository = _repo.CreateProxy<IssuedApiKey>();
            var prefix = ApiKeyGenerator.GetApiKey(7);
            var plainTextKey = $"{prefix}.{ApiKeyGenerator.GetApiKey(128)}";

            var salt = _apiKeyAuthSettings.ApiKeySalt;
            var convertedKey = ApiKeyGenerator.GeneratePasswordHash(plainTextKey, salt);

            var issue = new IssuedApiKey(
                _applicationUser,
                apiKeyRequest.Name,
                apiKeyRequest.Scopes,
                prefix,
                convertedKey);

            issuedApiKeyRepository.AddOrUpdate(issue);
            await _repo.CompleteAsync();

            var key = _mapper.Map<ApiKeyViewModel>(issue);
            key.PlainTextKey = plainTextKey;
            return key;
        }

        [HttpGet("subscription")]
        public async Task<ActionResult<SubscriptionViewModel>> GetSubscription() {
            return await Task.Run(() => Ok(_mapper.Map<SubscriptionViewModel>(_applicationUser)));
        }

        [HttpGet("opml-url")]
        public ActionResult<string> GetOpmlUrl() {
            return Content(
                $"{_appSettings.ApiUrl}/utility/opml/{_applicationUser.Slug}",
                "text/plain", Encoding.UTF8
            );
        }

        [HttpGet("limits")]
        public async Task<ActionResult<ProfileLimitsViewModel>> GetProfileLimits() {
            var entries = await _repo.Entries.GetAllForUserAsync(_applicationUser.Id);
            var quota = _applicationUser.DiskQuota ?? _storageSettings.DefaultUserQuota;
            var user = _mapper.Map<ApplicationUser, ProfileViewModel>(_applicationUser);

            var totalUsed = entries.Select(x => x.AudioFileSize)
                .Sum();

            var vm = new ProfileLimitsViewModel {
                StorageQuota = quota,
                StorageUsed = totalUsed,
                User = user
            };
            return Ok(vm);
        }
    }
}
