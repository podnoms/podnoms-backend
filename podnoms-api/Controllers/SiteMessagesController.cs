using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PodNoms.Common.Data.ViewModels.Resources;
using PodNoms.Common.Persistence;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Data.Models;

namespace PodNoms.Api.Controllers {
    [Route("[controller]")]
    [Authorize]
    public class SiteMessagesController : BaseAuthController {
        private readonly IRepository<SiteMessages> _repository;
        private readonly IRepoAccessor _repoAccessor;
        private readonly IMapper _mapper;

        public SiteMessagesController(
                        IHttpContextAccessor contextAccessor,
                        UserManager<ApplicationUser> userManager,
                        ILogger<SiteMessagesController> logger,
                        IRepository<SiteMessages> repository,
                        IRepoAccessor repoAccessor,
                        IMapper mapper) : base(contextAccessor, userManager, logger) {
            _repository = repository;
            _repoAccessor = repoAccessor;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<SiteMessageViewModel>> GetShowcaseForUser() {
            var candidate = await _repository.GetAll()
                .Where(r => r.Type.Equals(SiteMessageType.Showcase))
                .Where(r => r.StartDate <= System.DateTime.Today)
                .Where(r => r.EndDate >= System.DateTime.Today)
                .Where(r => r.IsActive)
                .SingleOrDefaultAsync();
            return _mapper.Map<SiteMessageViewModel>(candidate);
        }

        [HttpGet("banners")]
        public async Task<ActionResult<SiteMessageViewModel>> GetCurrentBannerMessage() {
            var candidate = await _repository.GetAll()
                .Where(r => r.Type.Equals(SiteMessageType.Banner))
                .Where(r => r.StartDate <= System.DateTime.Today)
                .Where(r => r.EndDate >= System.DateTime.Today)
                .Where(r => r.IsActive)
                .OrderByDescending(r => r.UpdateDate)
                .SingleOrDefaultAsync();
            return _mapper.Map<SiteMessageViewModel>(candidate);
        }
        [HttpPost("banners")]
        public async Task<ActionResult<SiteMessageViewModel>> AddBannerMessage([FromBody] SiteMessageViewModel model) {
            if (!ModelState.IsValid) return BadRequest("Invalid site message model");
            var message = _mapper.Map<SiteMessages>(model);
            message.StartDate = DateTime.Today;
            message.EndDate = DateTime.Today.AddMonths(1);
            message.IsActive = true;
            var result = _repository.AddOrUpdate(message);
            await _repoAccessor.CompleteAsync();
            return _mapper.Map<SiteMessageViewModel>(result);
        }
    }
}

