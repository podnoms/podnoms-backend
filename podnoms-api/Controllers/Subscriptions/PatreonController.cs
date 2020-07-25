using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PodNoms.Common.Data.Settings;
using PodNoms.Common.Data.ViewModels.Remote.Patreon;
using PodNoms.Common.Persistence;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Data.Models;

namespace PodNoms.Api.Controllers.Subscriptions {
    [Authorize]
    [Route("subscriptions/[controller]")]
    public class PatreonController : BaseAuthController {
        private readonly IRepository<PatreonToken> _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly PatreonSettings _patreonSettings;
        private readonly AppSettings _appSettings;

        public PatreonController(
            IHttpContextAccessor contextAccessor,
            UserManager<ApplicationUser> userManager,
            IRepository<PatreonToken> repository,
            IUnitOfWork unitOfWork,
            ILogger<PatreonController> logger, IHttpClientFactory httpClientFactory,
            IOptions<PatreonSettings> patreonSettings,
            IOptions<AppSettings> appSettings) : base(contextAccessor, userManager, logger) {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _httpClientFactory = httpClientFactory;
            _patreonSettings = patreonSettings.Value;
            _appSettings = appSettings.Value;
        }

        [HttpGet]
        public async Task<ActionResult<string>> GetToken([FromQuery] string accessKey) {
            // TODO Probably create a named Patreon HttpClient
            var redirectUri = $"{_appSettings.ApiUrl}/subscriptions/patreon/token";

            var url = $"/api/oauth2/token";
            var formBody = new FormUrlEncodedContent(new[] {
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("code", accessKey),
                new KeyValuePair<string, string>("client_id", _patreonSettings.ClientId),
                new KeyValuePair<string, string>("client_secret", _patreonSettings.ClientSecret),
                new KeyValuePair<string, string>("redirect_uri", $"{_appSettings.SiteUrl}/auth/redir/patreon")
            });

            var client = _httpClientFactory.CreateClient("patreon");
            client.DefaultRequestHeaders.Add(
                HttpRequestHeader.ContentType.ToString(),
                "application/x-www-form-urlencoded"
            );

            var response = await client.PostAsync(url, formBody);
            if (response.IsSuccessStatusCode) {
                var contents = await response.Content.ReadAsStringAsync();
                var received = JsonSerializer.Deserialize<PatreonAuthTokenResponseModel>(contents);

                var existingToken = await _repository.GetAll()
                    .FirstOrDefaultAsync(r => r.AppUserId == _applicationUser.Id);
                if (existingToken == null) {
                    existingToken = new PatreonToken {
                        AppUserId = _applicationUser.Id
                    };
                }

                existingToken.AccessToken = received.AccessToken;
                existingToken.ExpiresIn = received.ExpiresIn;
                existingToken.TokenType = received.TokenType;
                existingToken.FullName = received.FullName;
                existingToken.RefreshToken = received.RefreshToken;
                existingToken.Version = received.Version;
                existingToken.AppUserId = _applicationUser.Id;

                _repository.AddOrUpdate(existingToken);
                await _unitOfWork.CompleteAsync();

                return Content("You have successfully connected your Patreon account", "text/plain", Encoding.UTF8);
            }
            _logger.LogError($"Unable to connect to Patreon: {response.ReasonPhrase}");
            return BadRequest("Unable to connect your Patreon account at this time");
        }
    }
}
