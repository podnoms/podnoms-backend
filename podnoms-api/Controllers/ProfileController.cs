using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PodNoms.Common.Auth;
using PodNoms.Common.Data.Settings;
using PodNoms.Common.Data.ViewModels;
using PodNoms.Common.Data.ViewModels.Resources;
using PodNoms.Common.Persistence;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Data.Models;

namespace PodNoms.Api.Controllers {
    [Authorize]
    [Route("[controller]")]
    public class ProfileController : BaseAuthController {
        public IUnitOfWork _unitOfWork { get; }
        public IMapper _mapper { get; }
        private readonly IEntryRepository _entryRepository;
        private readonly IRepository<ApplicationUserSlugRedirects> _slugRedirectRepository;
        private readonly StorageSettings _storageSettings;

        public ProfileController(IMapper mapper, IUnitOfWork unitOfWork,
            IEntryRepository entryRepository, ILogger<ProfileController> logger,
            IRepository<ApplicationUserSlugRedirects> slugRedirectRepository,
            IOptions<StorageSettings> storageSettings,
            UserManager<ApplicationUser> userManager, IHttpContextAccessor contextAccessor) : base(contextAccessor, userManager, logger) {
            _entryRepository = entryRepository;
            _slugRedirectRepository = slugRedirectRepository;
            _mapper = mapper;
            _storageSettings = storageSettings.Value;
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public ActionResult<List<ProfileViewModel>> Get() {
            var result = _mapper.Map<ApplicationUser, ProfileViewModel>(_applicationUser);
            return Ok(new List<ProfileViewModel> { result });
        }

        [HttpPost]
        public async Task<ActionResult<ProfileViewModel>> Post([FromBody] ProfileViewModel item) {
            if (!string.IsNullOrEmpty(_applicationUser.Slug) && !_applicationUser.Slug.Equals(item.Slug)) {
                //item has changed, store the old slug for redirect purposes
                var existing = await _slugRedirectRepository
                    .GetAll()
                    .Where(r => r.ApplicationUser.Id == _applicationUser.Id && r.OldSlug == item.Slug)
                    .FirstOrDefaultAsync();
                if (existing is null) {
                    _slugRedirectRepository.AddOrUpdate(new ApplicationUserSlugRedirects {
                        ApplicationUser = _applicationUser,
                        OldSlug = _applicationUser.Slug
                    });
                    await _unitOfWork.CompleteAsync();
                }
            }
            _applicationUser.Slug = item.Slug;
            _applicationUser.FirstName = item.FirstName;
            _applicationUser.LastName = item.LastName;
            await _userManager.UpdateAsync(_applicationUser);
            var ret = _mapper.Map<ApplicationUser, ProfileViewModel>(_applicationUser);
            return Ok(ret);
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
            return NotFound();
        }
        [HttpGet("limits")]
        public async Task<ActionResult<ProfileLimitsViewModel>> GetProfileLimits() {
            var entries = await _entryRepository.GetAllForUserAsync(_applicationUser.Id);
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
