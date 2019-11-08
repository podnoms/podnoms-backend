using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Data.Models;

namespace PodNoms.Api.Controllers {
    [Authorize]
    public class LogsController : BaseAuthController {
        private readonly GenericRepository<ActivityLogPodcastEntry> _repository;

        public LogsController(IHttpContextAccessor contextAccessor,
                              UserManager<ApplicationUser> userManager,
                              ILogger<LogsController> logger,
                              GenericRepository<ActivityLogPodcastEntry> repository) : base(contextAccessor, userManager, logger) {
            _repository = repository;
        }

        [HttpGet]
        public async Task<ActionResult<List<ActivityLogPodcastEntry>>> GetEntryLogs(string entryId) {
            var logs = await _repository
                .GetAll()
                .Where(l => l.PodcastEntry.Id == Guid.Parse(entryId))
                .ToListAsync();
            return logs;
        }
    }
}
