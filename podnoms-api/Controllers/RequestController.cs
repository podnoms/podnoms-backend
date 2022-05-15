using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PodNoms.Common.Persistence;
using PodNoms.Data.Models;

namespace PodNoms.Api.Controllers {
    [Route("[controller]")]
    [Authorize(AuthenticationSchemes = "Bearer, PodNomsApiKey")]
    public class RequestController : BaseAuthController {
        private readonly IRepoAccessor _repo;

        public RequestController(
            IHttpContextAccessor contextAccessor,
            UserManager<ApplicationUser> userManager,
            ILogger<RequestController> logger,
            IRepoAccessor repo) : base(contextAccessor, userManager, logger) {
            _repo = repo;
        }

        [HttpPost("{url}")]
        public async Task<IActionResult> SubmitNonParsedUrl(string url) {
            var request = new UserRequest {
                FromUser = _applicationUser,
                RequestText = $"Unparseable URL: {url}"
            };
            _repo.CreateProxy<UserRequest>().AddOrUpdate(request);
            await _repo.CompleteAsync();

            return Ok(request);
        }
    }
}
