using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PodNoms.Common.Data.Settings;
using PodNoms.Common.Data.ViewModels.External;
using PodNoms.Common.Persistence;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Common.Services.PageParser;
using PodNoms.Data.Models;
using Slack.Webhooks;

namespace PodNoms.Api.Controllers.External {
    [Authorize(AuthenticationSchemes = "PodNomsApiKey")]
    [Route("pub/browserextension")]
    public class BrowserExtensionController : BaseAuthController {
        private readonly IConfiguration _options;
        private readonly IRepoAccessor _repo;
        private readonly ChatSettings _chatSettings;

        public BrowserExtensionController(
            ILogger<BrowserExtensionController> logger,
            IHttpContextAccessor contextAccessor,
            UserManager<ApplicationUser> userManager,
            IConfiguration options,
            IOptions<ChatSettings> chatSettings,
            IRepoAccessor repo) : base(contextAccessor, userManager, logger) {
            _options = options;
            _chatSettings = chatSettings.Value;
            _repo = repo;
        }

        [HttpGet("podcasts")]
        public async Task<ActionResult<List<BrowserExtensionPodcastViewModel>>> Get() {
            var podcasts = await _repo.Podcasts
                .GetAllForUserAsync(_applicationUser.Id);

            var ret = podcasts
                .Select(r => new BrowserExtensionPodcastViewModel {
                    Id = r.Id.ToString(),
                    Title = r.Title.ToString(),
                    ImageUrl = r.GetImageUrl(
                        _options.GetSection("StorageSettings")["ImageUrl"],
                        _options.GetSection("ImageFileStorageSettings")["ContainerName"])
                });
            return Ok(ret);
        }

        [HttpPost("flagurl")]
        public async Task<ActionResult> FlagUrl([FromQuery] string url) {
            var message = $"Please flag url: {url}";

            if (!string.IsNullOrEmpty(url)) {
                try {
                    var slackClient = new SlackClient(_chatSettings.SlackWebhookUrl);
                    var slackMessage = new SlackMessage {
                        Channel = "#userrequests",
                        Text =
                            $"{message}\n\nFrom: {_applicationUser.GetBestGuessName()}\nFromId: {_applicationUser.Id}\nFromEmail: {_applicationUser.Email}",
                        IconEmoji = Emoji.HearNoEvil,
                        Username = _applicationUser.Slug
                    };
                    await slackClient.PostAsync(slackMessage);
                } catch (Exception e) {
                    _logger.LogError("Error posting user flag url to slack");
                    _logger.LogError(e.Message);
                }

                var request = new UserRequest {
                    RequestText = message,
                    FromUser = _applicationUser
                };
                _repo.CreateProxy<UserRequest>().AddOrUpdate(request);
                await _repo.CompleteAsync();

                return Ok();
            }

            return BadRequest();
        }
    }
}
