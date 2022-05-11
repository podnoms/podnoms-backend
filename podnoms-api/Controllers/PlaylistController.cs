using System.Threading.Tasks;
using AutoMapper;
using Hangfire;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PodNoms.Data.Models;
using PodNoms.Common.Data.ViewModels.Resources;
using PodNoms.Common.Persistence;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Common.Services.Jobs;
using PodNoms.Common.Utils.Extensions;
using PodNoms.Common.Utils.RemoteParsers;

namespace PodNoms.Api.Controllers {
    [Route("[controller]")]
    public class PlaylistController : BaseAuthController {
        private readonly IYouTubeParser _youTubeParser;
        private readonly IRepoAccessor _repo;
        private readonly IMapper _mapper;

        public PlaylistController(
            IHttpContextAccessor contextAccessor, UserManager<ApplicationUser> userManager,
            IYouTubeParser youTubeParser, IRepoAccessor repo,
            IMapper mapper, ILogger logger) : base(contextAccessor,
            userManager, logger) {
            _youTubeParser = youTubeParser;
            _repo = repo;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<ActionResult<PlaylistViewModel>> Post([FromBody] PodcastEntryViewModel entry) {
            var podcast = await _repo.Podcasts.GetAsync(entry.PodcastId);
            if (podcast == null) {
                return NotFound();
            }

            if (string.IsNullOrEmpty(entry.SourceUrl)) {
                return BadRequest("SourceUrl is empty");
            }

            var sourceUrl = await _youTubeParser.ConvertUserToChannel(entry.SourceUrl, podcast.AppUserId);
            var playlist = new Playlist {
                Podcast = podcast,
                SourceUrl = sourceUrl
            };
            _repo.Playlists.AddOrUpdate(playlist);
            try {
                await _repo.CompleteAsync();
            } catch (DbUpdateException ex) {
                if (ex.InnerException != null && ex.InnerException.Message.Contains("IX_Playlists_SourceUrl")) {
                    return Conflict("This podcast is already monitoring this playlist");
                }

                return BadRequest("There was an error adding this playlist");
            }

            BackgroundJob.Enqueue<ProcessPlaylistsJob>(job => job.Execute(playlist.Id, null));
            return _mapper.Map<Playlist, PlaylistViewModel>(playlist);
        }
    }
}
