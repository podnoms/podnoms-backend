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
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Data.Models;

namespace PodNoms.Api.Controllers {
    [Authorize]
    [Route("[controller]")]
    public class LogsController : BaseAuthController {
        private readonly IMapper _mapper;
        private readonly IActivityLogPodcastEntryRepository _repository;

        public LogsController(IHttpContextAccessor contextAccessor,
                              UserManager<ApplicationUser> userManager,
                              ILogger<LogsController> logger,
                              IMapper mapper,
                              IActivityLogPodcastEntryRepository repository) : base(contextAccessor, userManager, logger) {
            _mapper = mapper;
            _repository = repository;
        }

        [HttpGet]
        public async Task<ActionResult<List<ActivityLogPodcastEntryViewModel>>> GetEntryLogs(string entryId) {
            var logs = await _repository.GetForEntry(entryId);

            return _mapper.Map<List<ActivityLogPodcastEntry>, List<ActivityLogPodcastEntryViewModel>>(logs);
        }
    }
}
