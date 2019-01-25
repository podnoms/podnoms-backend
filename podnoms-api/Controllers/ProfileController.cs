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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PodNoms.Data.Models;
using PodNoms.Common.Auth;
using PodNoms.Common.Data.Settings;
using PodNoms.Common.Data.ViewModels.Resources;
using PodNoms.Common.Persistence;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Common.Data.ViewModels;

namespace PodNoms.Api.Controllers {
    [Authorize]
    [Route("[controller]")]
    public class ProfileController : BaseAuthController {
        public IUnitOfWork _unitOfWork { get; }
        public IMapper _mapper { get; }
        private readonly IEntryRepository _entryRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly StorageSettings _storageSettings;

        public ProfileController(IMapper mapper, IUnitOfWork unitOfWork,
            IEntryRepository entryRepository, IPaymentRepository paymentRepository, ILogger<ProfileController> logger,
            IOptions<StorageSettings> storageSettings,
            UserManager<ApplicationUser> userManager, IHttpContextAccessor contextAccessor)
            : base(contextAccessor, userManager, logger) {
            _entryRepository = entryRepository;
            this._paymentRepository = paymentRepository;
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
            _applicationUser.Slug = item.Slug;
            _applicationUser.FirstName = item.FirstName;
            _applicationUser.LastName = item.LastName;
            await _userManager.UpdateAsync(_applicationUser);
            var ret = _mapper.Map<ApplicationUser, ProfileViewModel>(_applicationUser);
            return Ok(ret);
        }

        [HttpGet("checkslug/{slug}")]
        public async Task<ActionResult<bool>> CheckSlug(string slug) {
            var slugValid = await _userManager.CheckSlug(slug)
                            || (slug.Equals(_applicationUser.Slug));
            return Ok(slugValid);
        }

        [HttpGet("limits")]
        public async Task<ActionResult<ProfileLimitsViewModel>> GetProfileLimits() {
            var entries = await _entryRepository.GetAllForUserAsync(_applicationUser.Id);
            var quota = _applicationUser.DiskQuota ?? _storageSettings.DefaultUserQuota;
            var user = _mapper.Map<ApplicationUser, ProfileViewModel>(_applicationUser);

            var totalUsed = entries.Select(x => x.AudioFileSize)
                .Sum();

            var vm = new ProfileLimitsViewModel
            {
                StorageQuota = quota,
                StorageUsed = totalUsed,
                User = user
            };
            return Ok(vm);
        }
        [HttpGet("payments")]
        public ActionResult<IEnumerable<PaymentLogViewModel>> GetPayments() {
            var payments =
                _paymentRepository.GetAll()
                .Where(r => r.AppUser.Id == _applicationUser.Id)
                .OrderByDescending(e => e.StartDate)
                .Select(e => new PaymentLogViewModel
                {
                    TransactionId = e.TransactionId,
                    Type = "advanced",
                    StartDate = e.StartDate,
                    EndDate = e.EndDate,
                    ReceiptURL = e.ReceiptURL
                });

            return Ok(payments);
        }
    }
}