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

        public AudioStreamController(
            IHttpContextAccessor contextAccessor,
            UserManager<ApplicationUser> userManager,
            ILogger logger) : base(contextAccessor, userManager, logger) {
        }
    }
}
