using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PodNoms.Data.Models;


namespace PodNoms.Api.Persistence {
    public interface IEntryRepository : IRepository<PodcastEntry> {
        Task<IEnumerable<PodcastEntry>> GetAllForSlugAsync(string podcastSlug);
        Task<IEnumerable<PodcastEntry>> GetAllForUserAsync(string userId);
        Task LoadPodcastAsync(PodcastEntry entry);
    }
    public class EntryRepository : GenericRepository<PodcastEntry>, IEntryRepository {
        public EntryRepository(PodNomsDbContext context, ILogger<EntryRepository> logger) : base(context, logger) {
        }

        public new async Task<PodcastEntry> GetAsync(Guid id) {
            var ret = await GetAll()
                .Where(p => p.Id == id)
                .Include(p => p.Podcast)
                .Include(p => p.Podcast.AppUser)
                .FirstOrDefaultAsync();
            return ret;
        }
        public async Task<IEnumerable<PodcastEntry>> GetAllForSlugAsync(string podcastSlug) {
            var entries = await GetAll()
                .Where(e => e.Podcast.Slug == podcastSlug)
                .Include(e => e.Podcast)
                .ToListAsync();
            return entries;
        }
        public async Task<IEnumerable<PodcastEntry>> GetAllForUserAsync(string userId) {
            var entries = await GetAll()
                .Where(e => e.Podcast.AppUser.Id == userId)
                .Include(e => e.Podcast)
                .ToListAsync();
            return entries;
        }

        public async Task LoadPodcastAsync(PodcastEntry entry) {
            await GetContext().Entry(entry)
                   .Reference(e => e.Podcast)
                   .LoadAsync();
        }
    }
}