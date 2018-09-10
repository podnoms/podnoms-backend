using System.Threading.Tasks;
using AutoMapper;
using Hangfire;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PodNoms.Data.Models;
using PodNoms.Data.Models.ViewModels;
using PodNoms.Api.Persistence;
using PodNoms.Api.Services.Auth;
using PodNoms.Api.Services.Jobs;

namespace PodNoms.Api.Controllers {
    [Route("[controller]")]
    public class PlaylistController : BaseAuthController {
        private readonly IPlaylistRepository _playlistRepository;
        private readonly IPodcastRepository _podcastRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public PlaylistController(IPlaylistRepository playlistRepository, IPodcastRepository podcastRepository,
                IHttpContextAccessor contextAccessor, UserManager<ApplicationUser> userManager,
                IUnitOfWork unitOfWork, IMapper mapper, ILogger<PlaylistController> logger) : base(contextAccessor, userManager, logger) {
            this._playlistRepository = playlistRepository;
            this._podcastRepository = podcastRepository;
            this._unitOfWork = unitOfWork;
            this._mapper = mapper;
        }

        [HttpPost]
        public async Task<ActionResult<Playlist>> Post([FromBody] PodcastEntryViewModel entry) {
            var podcast = await _podcastRepository.GetAsync(entry.PodcastId);
            if (podcast != null) {
                var playlist = new Playlist() {
                    Podcast = podcast,
                    SourceUrl = entry.SourceUrl
                };
                _playlistRepository.AddOrUpdate(playlist);
                await _unitOfWork.CompleteAsync();
                BackgroundJob.Enqueue<ProcessPlaylistsJob>(job => job.Execute(playlist.Id));
                return Ok(playlist);
            }
            return NotFound();
        }
    }
}
