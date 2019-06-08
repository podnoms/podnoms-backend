using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PodNoms.Data.Models;

namespace PodNoms.Common.Persistence.Repositories {
    public interface IPlaylistRepository : IRepository<Playlist> {
        IEnumerable<ParsedPlaylistItem> GetParsedItems ();
        Task<ParsedPlaylistItem> GetParsedItem (string itemId, Guid playlistId);
        Task<List<ParsedPlaylistItem>> GetUnprocessedItems ();
    }
    public class PlaylistRepository : GenericRepository<Playlist>, IPlaylistRepository {

        public PlaylistRepository (
            PodNomsDbContext context,
            ILogger<PlaylistRepository> logger) : base (context, logger) { }

        public new async Task<Playlist> GetAsync (Guid id) {
            return await GetContext ().Playlists
                .Include (i => i.ParsedPlaylistItems)
                .Include (i => i.PodcastEntries)
                .Include (p => p.Podcast)
                .Include (u => u.Podcast.AppUser)
                .SingleOrDefaultAsync (i => i.Id == id);
        }

        public IEnumerable<ParsedPlaylistItem> GetParsedItems () {
            return GetContext ().ParsedPlaylistItems
                .Include (i => i.Playlist)
                .Include (i => i.Playlist.Podcast)
                .Include (i => i.Playlist.Podcast.AppUser);
        }

        public async Task<ParsedPlaylistItem> GetParsedItem (string itemId, Guid playlistId) {
            return await GetContext ().ParsedPlaylistItems
                .Include (i => i.Playlist)
                .Include (i => i.Playlist.Podcast)
                .Include (i => i.Playlist.Podcast.AppUser)
                .SingleOrDefaultAsync (i => i.VideoId == itemId && i.Playlist.Id == playlistId);
        }

        public async Task<List<ParsedPlaylistItem>> GetUnprocessedItems () {
            return await GetContext ().ParsedPlaylistItems
                .Where (p => p.IsProcessed == false)
                .Include (i => i.Playlist)
                .Include (i => i.Playlist.Podcast)
                .Include (i => i.Playlist.Podcast.AppUser)
                .ToListAsync ();
        }
    }
}
