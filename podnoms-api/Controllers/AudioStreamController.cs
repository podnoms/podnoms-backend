using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Data.Models;

namespace PodNoms.Api.Controllers {
    [Authorize]
    [Route("/podcast/{slug}/audioupload")]
    public class AudioStreamController : BaseAuthController {
        private readonly IEntryRepository _entryRepository;

        public AudioStreamController(
            IHttpContextAccessor contextAccessor,
            UserManager<ApplicationUser> userManager,
            ILogger logger, IEntryRepository entryRepository) : base(contextAccessor, userManager, logger) {
            _entryRepository = entryRepository;
        }
    }
}
