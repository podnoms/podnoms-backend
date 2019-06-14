using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Filters;
using PodNoms.Data.Models;

namespace PodNoms.Common.Services.Middleware {
    public class UserLoggingFilter : IAsyncActionFilter {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _contextAccessor;

        public UserLoggingFilter (UserManager<ApplicationUser> userManager, IHttpContextAccessor contextAccessor) {
            _userManager = userManager;
            _contextAccessor = contextAccessor;
        }
        public async Task OnActionExecutionAsync (ActionExecutingContext context, ActionExecutionDelegate next) {
            string userId = null;

            var claimsIdentity = (ClaimsIdentity) context.HttpContext.User.Identity;
            var userIdClaim = claimsIdentity.Claims.SingleOrDefault (c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim != null) {
                userId = userIdClaim.Value;
            }

            var user = await _userManager.FindByNameAsync (userId);

            user.IpAddress = _contextAccessor.HttpContext.Connection.RemoteIpAddress.ToString ();
            user.LastSeen = System.DateTime.Now;
            await _userManager.UpdateAsync (user);

            var resultContext = await next ();
        }
    }
}
