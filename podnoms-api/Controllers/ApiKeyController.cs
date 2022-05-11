using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PodNoms.Common.Data.ViewModels.Resources;
using PodNoms.Common.Persistence;
using PodNoms.Data.Models;

namespace PodNoms.Api.Controllers {
    [Authorize]
    [Route("[controller]")]
    public class ApiKeyController : BaseAuthController {
        private readonly IMapper _mapper;
        private readonly IRepoAccessor _repo;

        public ApiKeyController(IHttpContextAccessor contextAccessor, UserManager<ApplicationUser> userManager,
            ILogger logger, IMapper mapper,
            IRepoAccessor repo) :
            base(contextAccessor, userManager, logger) {
            _mapper = mapper;
            _repo = repo;
        }

        [HttpPost("addservicekey")]
        public async Task<ActionResult<ServiceApiKeyViewModel>> AddServiceApiKey(
            [FromBody] ServiceApiKeyViewModel apiKey) {
            var key = _mapper.Map<ServiceApiKey>(apiKey);
            if (key == null) {
                return BadRequest();
            }

            key.User = _applicationUser;

            var newKey = _repo.ApiKey.AddOrUpdate(key);
            await _repo.CompleteAsync();

            return _mapper.Map<ServiceApiKeyViewModel>(newKey);
        }
    }
}
