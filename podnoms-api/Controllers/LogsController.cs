using System;
using System.Collections.Generic;
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
    public class LogsController : BaseAuthController {
        private readonly IRepoAccessor _repo;
        private readonly IMapper _mapper;

        public LogsController(IHttpContextAccessor contextAccessor,
            UserManager<ApplicationUser> userManager,
            ILogger logger,
            IRepoAccessor repo,
            IMapper mapper) : base(contextAccessor, userManager, logger) {
            _repo = repo;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<List<ActivityLogPodcastEntryViewModel>>> GetEntryLogs(string entryId) {
            var logs = await _repo.ActivityLogPodcastEntry.GetForEntry(entryId);
            return _mapper.Map<List<ActivityLogPodcastEntry>, List<ActivityLogPodcastEntryViewModel>>(logs);
        }
    }
}
