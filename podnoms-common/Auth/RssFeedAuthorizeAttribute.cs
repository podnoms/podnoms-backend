using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Common.Utils.Crypt;
using PodNoms.Data.Models;

namespace PodNoms.Common.Auth {
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RssFeedAuthorizeAttribute : TypeFilterAttribute {
        public RssFeedAuthorizeAttribute()
            : base(typeof(RssFeedAuthorizeFilter)) { }
    }

    public class RssFeedAuthorizeFilter : IAuthorizationFilter {
        private const string Realm = "podnoms.com";

        private readonly IPodcastRepository _podcastRepository;
        private readonly ILogger _logger;

        public RssFeedAuthorizeFilter(IPodcastRepository podcastRepository, ILogger<RssFeedAuthorizeFilter> logger) {
            _podcastRepository = podcastRepository;
            _logger = logger;
        }

        public void OnAuthorization(AuthorizationFilterContext context) {
            var pathsegments = context.HttpContext.Request.Path.Value.Split("/");

            if (pathsegments.Length < 2) {
                context.Result = new StatusCodeResult(400);
                return;
            }

            //user is second from last
            var userSlug = pathsegments[pathsegments.Length - 2];
            //podcast slug is last
            var podcastSlug = pathsegments[pathsegments.Length - 1];

            //get the podcast
            var podcast = _podcastRepository
                .GetAll().FirstOrDefault(
                    p => p.AppUser.Slug == userSlug &&
                         p.Slug == podcastSlug && (p.Private));
            if (podcast is null)
                return;

            string authHeader = context.HttpContext.Request.Headers["Authorization"];
            if (authHeader != null && authHeader.StartsWith("Basic ")) {
                // Get the encoded username and password
                var encodedUsernamePassword =
                    authHeader.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries)[1]?.Trim();
                // Decode from Base64 to string
                var decodedUsernamePassword =
                    Encoding.UTF8.GetString(Convert.FromBase64String(encodedUsernamePassword));
                // Split username and password
                var username = decodedUsernamePassword.Split(':', 2)[0];
                var password = decodedUsernamePassword.Split(':', 2)[1];
                // Check if login is correct
                if (IsAuthorized(podcast, username, password)) {
                    return;
                }
            }

            // Return authentication type (causes browser to show login dialog)
            context.HttpContext.Response.Headers["WWW-Authenticate"] = "Basic";
            // Add realm if it is not null
            context.HttpContext.Response.Headers["WWW-Authenticate"] += $" realm=\"{Realm}\"";

            // Return unauthorized
            context.Result = new UnauthorizedResult();
        }

        // Make your own implementation of this
        public bool IsAuthorized(Podcast podcast, string username, string password) {
            var record = _podcastRepository
                .GetAll()
                .SingleOrDefault(r => r.AuthUserName.Equals(username));
            if (record is null)
                return false;

            var pwd = Encoding.ASCII.GetBytes(password);
            var checkHash = PBKDFGenerators.GenerateHash(pwd, podcast.AuthPasswordSalt);
            return podcast.AuthPassword.SequenceEqual(checkHash);
        }
    }
}
