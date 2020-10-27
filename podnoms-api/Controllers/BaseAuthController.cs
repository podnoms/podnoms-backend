using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PodNoms.Common.Auth;
using PodNoms.Data.Models;

namespace PodNoms.Api.Controllers {
    [Authorize]
    public abstract class BaseAuthController : BaseController {
        private readonly ClaimsPrincipal _caller;
        protected readonly UserManager<ApplicationUser> _userManager;

        protected readonly ApplicationUser _applicationUser;

        //TODO: IMPORTANT
        //This should be the IHttpContextAccessor 
        //and should be used like _httpContextAccessor.HttpContext AT POINT OF USE
        protected readonly IHttpContextAccessor _httpContextAccessor;
        protected string UserId => _applicationUser.Id;

        protected BaseAuthController(IHttpContextAccessor contextAccessor,
            UserManager<ApplicationUser> userManager,
            ILogger logger) : base(logger) {
            _caller = contextAccessor?.HttpContext?.User;
            _userManager = userManager;
            _httpContextAccessor = contextAccessor;
            if (_caller == null) {
                throw new NotAuthorisedException("Unable to find authorised user for this request");
            }

            try {
                if (_caller?.Identity != null && !_caller.Identity.IsAuthenticated) {
                    return;
                }

                var userId = _caller.Claims.Single(c => c.Type == "id")?.Value;
                _applicationUser = userManager.FindByIdAsync(userId).Result;
                if (_applicationUser is null) {
                    throw new NotAuthorisedException("Unable to find authorised user for this request");
                }
            } catch (System.InvalidOperationException ex) {
                _logger.LogError($"Error constructing BaseAuthController: \n{ex.Message}");
            }
        }
    }
}
