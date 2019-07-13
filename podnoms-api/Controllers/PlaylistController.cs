using System;
using System.Threading.Tasks;
using AutoMapper;
using Hangfire;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PodNoms.Data.Models;
using PodNoms.Common.Auth;
using PodNoms.Common.Data.ViewModels.Resources;
using PodNoms.Common.Persistence;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Common.Services.Jobs;

namespace PodNoms.Api.Controllers {
    [Route("[controller]")]
    public class PlaylistController : BaseAuthController {
        private readonly IPlaylistRepository _playlistRepository;
        private readonly IPodcastRepository _podcastRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public PlaylistController(IPlaylistRepository playlistRepository, IPodcastRepository podcastRepository,
            IHttpContextAccessor contextAccessor, UserManager<ApplicationUser> userManager,
            IUnitOfWork unitOfWork, IMapper mapper, ILogger<PlaylistController> logger) : base(contextAccessor,
            userManager, logger) {
            _playlistRepository = playlistRepository;
            _podcastRepository = podcastRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<ActionResult<PlaylistViewModel>> Post([FromBody] PodcastEntryViewModel entry) {
            var podcast = await _podcastRepository.GetAsync(entry.PodcastId);
            if (podcast == null) {
                return NotFound();
            }

            var playlist = new Playlist {
                Podcast = podcast,
                SourceUrl = entry.SourceUrl
            };
            _playlistRepository.AddOrUpdate(playlist);
            try {
                await _unitOfWork.CompleteAsync();
            } catch (DbUpdateException ex) {
                if (ex.InnerException.Message.Contains("IX_Playlists_SourceUrl")) {
                    return Conflict("This podcast is already monitoring this playlist");
                }

                return BadRequest("There was an error adding this playlist");
            }

            BackgroundJob.Enqueue<ProcessPlaylistsJob>(job => job.Execute(playlist.Id));
            return _mapper.Map<Playlist, PlaylistViewModel>(playlist);
        }
    }
}
