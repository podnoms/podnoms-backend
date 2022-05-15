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
using PodNoms.Data.Models;

namespace PodNoms.Api.Controllers {
    [Authorize]
    [Route("[controller]")]
    public class SiteMessagesController : BaseAuthController {
        private readonly IRepoAccessor _repo;
        private readonly IMapper _mapper;

        public SiteMessagesController(
            ILogger<SiteMessagesController> logger,
            UserManager<ApplicationUser> userManager,
            IHttpContextAccessor contextAccessor,
            IRepoAccessor repo,
            IMapper mapper) : base(contextAccessor, userManager, logger) {
            _repo = repo;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<SiteMessageViewModel>> GetShowcaseForUser() {
            var candidate = await _repo.CreateProxy<SiteMessages>().GetAll()
                .Where(r => r.Type.Equals(SiteMessageType.Showcase))
                .Where(r => r.StartDate <= DateTime.Today)
                .Where(r => r.EndDate >= DateTime.Today)
                .Where(r => r.IsActive)
                .SingleOrDefaultAsync();
            return _mapper.Map<SiteMessageViewModel>(candidate);
        }

        [HttpGet("banners")]
        public async Task<ActionResult<SiteMessageViewModel>> GetCurrentBannerMessage() {
            var candidate = await _repo.CreateProxy<SiteMessages>().GetAll()
                .Where(r => r.Type.Equals(SiteMessageType.Banner))
                .Where(r => r.StartDate <= DateTime.Today)
                .Where(r => r.EndDate >= DateTime.Today)
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
            var result = _repo.CreateProxy<SiteMessages>().AddOrUpdate(message);
            await _repo.CompleteAsync();
            return _mapper.Map<SiteMessageViewModel>(result);
        }
    }
}
