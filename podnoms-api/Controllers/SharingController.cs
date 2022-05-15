using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PodNoms.Common.Data.Settings;
using PodNoms.Common.Data.ViewModels.Resources;
using PodNoms.Common.Persistence;
using PodNoms.Common.Services;
using PodNoms.Data.Models;

namespace PodNoms.Api.Controllers {
    [Route("[controller]")]
    public class SharingController : BaseAuthController {
        private readonly IRepoAccessor _repo;
        private readonly IMailSender _mailSender;
        private readonly SharingSettings _sharingSettings;

        public SharingController(
            IHttpContextAccessor contextAccessor,
            UserManager<ApplicationUser> userManager,
            IRepoAccessor repo,
            IMailSender mailSender,
            IOptions<SharingSettings> sharingSettings,
            ILogger<SharingController> logger) : base(contextAccessor, userManager, logger) {
            this._repo = repo;
            this._mailSender = mailSender;
            this._sharingSettings = sharingSettings.Value;
        }

        [HttpPost]
        public async Task<ActionResult<SharingResultViewModel>> ShareToEmail([FromBody] SharingViewModel model) {
            if (string.IsNullOrEmpty(model.Id) || string.IsNullOrEmpty(model.Email)) {
                return BadRequest();
            }

            try {
                var entry = await _repo.Entries.GetAsync(_applicationUser.Id, model.Id);

                if (entry is null) {
                    return NotFound();
                }

                var link = await _repo.Entries.CreateNewSharingLink(model);
                if (link != null) {
                    await _repo.CompleteAsync();
                    var url = Flurl.Url.Combine(_sharingSettings.BaseUrl, link.LinkId);
                    await this._mailSender.SendEmailAsync(
                        model.Email,
                        $"{_applicationUser.GetBestGuessName()} shared a link with you",
                        new MailDropin {
                            username = model.Email.Split('@')[0], //bite me!
                            message =
                                $"<p>{_applicationUser.GetBestGuessName()} wants to share an audio file with you!</p><br />" +
                                $"<p>{model.Message}</p>",
                            buttonmessage = "Let me at it!!",
                            buttonaction = url
                        });
                    return Ok();
                }
            } catch (Exception e) {
                _logger.LogError(e.Message);
            }

            return StatusCode(500);
        }

        [HttpPost("generatelink")]
        public async Task<ActionResult<SharingResultViewModel>> GenerateSharingLink([FromBody] SharingViewModel model) {
            var entry = await _repo.Entries.GetAsync(_applicationUser.Id, model.Id);
            if (entry == null)
                return NotFound();

            var share = await _repo.Entries.CreateNewSharingLink(model);
            if (share == null) {
                return BadRequest();
            }

            await _repo.CompleteAsync();
            return Ok(new SharingResultViewModel {
                Id = model.Id,
                ValidFrom = model.ValidFrom,
                ValidTo = model.ValidTo,
                Url = Flurl.Url.Combine(_sharingSettings.BaseUrl, share.LinkId)
            });
        }
    }
}
