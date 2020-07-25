using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;
using PodNoms.Data.Models;

namespace PodNoms.Common.Persistence.Repositories {
    public interface IPlaylistRepository : IRepository<Playlist> {
        Task<DateTime> GetCutoffDate(Guid playlistId);
    }
    public class PlaylistRepository : GenericRepository<Playlist>, IPlaylistRepository {

        public PlaylistRepository(
            PodNomsDbContext context,
            ILogger<PlaylistRepository> logger) : base(context, logger) { }


        public new async Task<Playlist> GetAsync(Guid id) {
            return await GetContext().Playlists
                .Include(i => i.PodcastEntries)
                .Include(p => p.Podcast)
                .Include(u => u.Podcast.AppUser)
                .Include(u => u.Podcast.AppUser.AccountSubscriptions)
                .SingleOrDefaultAsync(i => i.Id == id);
        }

        public async Task<DateTime> GetCutoffDate(Guid playlistId) {
            //get the latest processed episode so we can filter results
            //in the various parsers
            var playlist = await GetAsync(playlistId);
            var item = playlist
                .PodcastEntries
                .OrderByDescending(p => p.SourceCreateDate)
                .FirstOrDefault();
            return item?.CreateDate ?? DateTime.MinValue;
        }
    }
}
