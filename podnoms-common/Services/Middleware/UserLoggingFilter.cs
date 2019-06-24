using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using PodNoms.Data.Models;

namespace PodNoms.Common.Services.Middleware {
    public class UserLoggingFilter : IAsyncActionFilter {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger _logger;

        public UserLoggingFilter(UserManager<ApplicationUser> userManager, ILogger<UserLoggingFilter> logger) {
            _userManager = userManager;
            _logger = logger;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next) {
            if (context.HttpContext.User.Identity.IsAuthenticated) {

                string userId = null;

                var claimsIdentity = (ClaimsIdentity)context.HttpContext.User.Identity;
                var userIdClaim = claimsIdentity.Claims.SingleOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
                if (userIdClaim != null) {
                    userId = userIdClaim.Value;
                }

                var user = await _userManager.FindByNameAsync(userId);
                if (user != null) {
                    var header = context.HttpContext.Request.Headers["X-Forwarded-For"].SingleOrDefault();
                    if (header != null) {
                        //might be coming to us from a proxy
                        user.IpAddress = header;
                    } else {
                        user.IpAddress = context.HttpContext.Connection.RemoteIpAddress.ToString();
                    }
                    user.LastSeen = System.DateTime.Now;
                    await _userManager.UpdateAsync(user);
                }
            }
            var resultContext = await next();
        }
    }

}
