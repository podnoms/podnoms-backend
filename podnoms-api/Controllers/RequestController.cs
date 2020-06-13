using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PodNoms.Common.Persistence;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Data.Models;

namespace PodNoms.Api.Controllers {
    [Route("[controller]")]
    [Authorize(AuthenticationSchemes = "Bearer, PodNomsApiKey")]
    public class RequestController : BaseAuthController {
        private readonly IRepository<UserRequest> _repository;
        private readonly IUnitOfWork _unitOfWork;

        public RequestController(
                    IHttpContextAccessor contextAccessor,
                    UserManager<ApplicationUser> userManager,
                    ILogger logger, IRepository<UserRequest> repository,
                    IUnitOfWork unitOfWork) : base(contextAccessor, userManager, logger) {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        [HttpPost("{url}")]
        public async Task<IActionResult> SubmitNonParsedUrl(string url) {
            var request = new UserRequest {
                FromUser = _applicationUser,
                RequestText = $"Unparseable URL: {url}"
            };
            _repository.AddOrUpdate(request);
            await _unitOfWork.CompleteAsync();

            return Ok(request);
        }
    }
}
