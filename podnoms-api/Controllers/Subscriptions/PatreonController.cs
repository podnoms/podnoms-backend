using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PodNoms.Common.Data.Settings;
using PodNoms.Data.Models;

namespace PodNoms.Api.Controllers.Subscriptions {
    [Authorize]
    [Route("subscriptions/[controller]")]
    public class PatreonController : BaseAuthController {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly PatreonSettings _patreonSettings;
        private readonly AppSettings _appSettings;

        public PatreonController(
                IHttpContextAccessor contextAccessor,
                UserManager<ApplicationUser> userManager,
                ILogger<PatreonController> logger, IHttpClientFactory httpClientFactory,
                IOptions<PatreonSettings> patreonSettings,
                IOptions<AppSettings> appSettings) : base(contextAccessor, userManager, logger) {
            _httpClientFactory = httpClientFactory;
            _patreonSettings = patreonSettings.Value;
            _appSettings = appSettings.Value;
        }

        [HttpGet]
        public async Task<IActionResult> GetToken([FromQuery] string accessKey) {
            // TODO Probably create a named Patreon HttpClient
            var redirectUri = $"{_appSettings.ApiUrl}/subscriptions/patreon/token";
            var url = $"https://www.patreon.com/api/oauth2/token?code={accessKey}&grant_type=authorization_code&client_id={_patreonSettings.ClientId}&client_secret={_patreonSettings.ClientSecret}&redirect_uri={redirectUri}";
            // var formBody = new FormUrlEncodedContent(new[]{
            //     new KeyValuePair<string, string>("grant_type", "authorization_code"),
            //     new KeyValuePair<string, string>("code", accessKey),
            //     new KeyValuePair<string, string>("client_id", _patreonSettings.ClientId),
            //     new KeyValuePair<string, string>("client_secret", _patreonSettings.ClientSecret),
            //     new KeyValuePair<string, string>("redirect_uri", "https://dev.pdnm.be:4200/auth/redir/patreon")
            // });

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add(
                HttpRequestHeader.ContentType.ToString(),
                "application/x-www-form-urlencoded"
            );

            var response = await client.PostAsync(url, null);
            if (response.IsSuccessStatusCode) {
                return Ok();
            }

            _logger.LogError($"Unable to connect to Patreon: {response.ReasonPhrase}");
            return BadRequest("Unable to connect your Patreon account at this time");
        }

        [HttpGet("token")]
        [AllowAnonymous]
        public async Task<IActionResult> ReceiveToken([FromQuery] string code, [FromQuery] string state) {
            _logger.LogDebug($"Patreon Token {code} State: {state}");
            return Ok();
        }
    }
}
