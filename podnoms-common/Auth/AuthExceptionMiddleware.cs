using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace PodNoms.Common.Auth {
    /// <summary>
    /// Catch all middleware to ensure any requests
    /// where _applicationUser is null returns a 401
    /// </summary>
    public class AuthExceptionMiddleware {
        private readonly RequestDelegate _next;

        public AuthExceptionMiddleware(RequestDelegate next) {
            _next = next;
        }

        public async Task Invoke(HttpContext context) {
            try {
                await _next(context);
            } catch (NotAuthorisedException ex) {
                context.Response.ContentType = "text/plain";
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                await context.Response.WriteAsync(ex.Message);
            }
        }
    }
}
