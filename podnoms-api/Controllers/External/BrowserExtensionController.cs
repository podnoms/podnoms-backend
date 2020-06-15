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
using PodNoms.Common.Data.ViewModels.External;
using PodNoms.Common.Persistence;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Common.Services.PageParser;
using PodNoms.Data.Models;

namespace PodNoms.Api.Controllers.External {
    [Authorize(AuthenticationSchemes = "PodNomsApiKey")]
    [Route("pub/browserextension")]
    public class BrowserExtensionController : BaseAuthController {
        private readonly IPodcastRepository _podcastRepository;
        private readonly IConfiguration _options;
        private readonly GenericRepository<UserRequest> _userRequestRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPageParser _parser;

        public BrowserExtensionController(
            ILogger<BrowserExtensionController> logger,
            IPodcastRepository podcastRepository,
            IHttpContextAccessor contextAccessor,
            UserManager<ApplicationUser> userManager,
            IConfiguration options,
            GenericRepository<UserRequest> userRequestRepository,
            IUnitOfWork unitOfWork,
            IPageParser parser) : base(contextAccessor, userManager, logger) {
            _podcastRepository = podcastRepository;
            _options = options;
            _userRequestRepository = userRequestRepository;
            _unitOfWork = unitOfWork;
            this._parser = parser;
        }

        [HttpGet("podcasts")]
        [EnableCors("BrowserExtensionPolicy")]
        public async Task<ActionResult<List<BrowserExtensionPodcastViewModel>>> Get() {
            var podcasts = await _podcastRepository
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

        [HttpGet("flagurl/{url}")]
        [EnableCors("BrowserExtensionPolicy")]
        public async Task<ActionResult> FlagUrl(string url) {
            var request = new UserRequest() {
                RequestText = $"Please flag url: {url}",
                FromUser = _applicationUser
            };
            _userRequestRepository.AddOrUpdate(request);
            await _unitOfWork.CompleteAsync();
            return Ok();
        }
    }
}
