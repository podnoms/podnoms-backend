using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PodNoms.Common.Data.ViewModels.Resources;
using PodNoms.Common.Persistence;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Data.Models;

namespace PodNoms.Api.Controllers {
    [Authorize]
    [Route("[controller]")]
    public class ApiKeyController : BaseAuthController {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IServiceApiKeyRepository _repository;

        public ApiKeyController(IHttpContextAccessor contextAccessor, UserManager<ApplicationUser> userManager,
            ILogger<ApiKeyController> logger, IMapper mapper, IServiceApiKeyRepository repository,
            IUnitOfWork unitOfWork) :
            base(contextAccessor, userManager, logger) {
            _mapper = mapper;
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        [HttpPost("addservicekey")]
        public async Task<ActionResult<ServiceApiKeyViewModel>> AddServiceApiKey(
            [FromBody] ServiceApiKeyViewModel apiKey) {
            var key = _mapper.Map<ServiceApiKey>(apiKey);
            if (key == null) {
                return BadRequest();
            }

            key.User = _applicationUser;

            var newKey = _repository.AddOrUpdate(key);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<ServiceApiKeyViewModel>(newKey);
        }
    }
}
