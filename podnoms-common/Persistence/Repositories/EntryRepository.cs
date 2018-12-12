using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PodNoms.Data.Models;

namespace PodNoms.Common.Persistence.Repositories {
    public interface IEntryRepository : IRepository<PodcastEntry> {
        Task<PodcastEntry> GetAsync(string userId, string entryId);
        Task<IEnumerable<PodcastEntry>> GetAllForSlugAsync(string podcastSlug);
        Task<IEnumerable<PodcastEntry>> GetAllForUserAsync(string userId);
        Task<PodcastEntry> GetFeaturedEpisode(Podcast podcast);
        Task LoadPodcastAsync(PodcastEntry entry);
    }
    public class EntryRepository : GenericRepository<PodcastEntry>, IEntryRepository {
        public EntryRepository(PodNomsDbContext context, ILogger<EntryRepository> logger) : base(context, logger) {
        }

        public override PodcastEntry AddOrUpdate(PodcastEntry entry){
            GetContext().Entry<PodcastEntry>(entry).Property(x => x.AudioUrl).IsModified = false;
            return base.AddOrUpdate(entry);
        }
        public new async Task<PodcastEntry> GetAsync(Guid id) {
            var ret = await GetAll()
                .Where(p => p.Id == id)
                .Include(p => p.Podcast)
                .Include(p => p.Podcast.AppUser)
                .FirstOrDefaultAsync();
            return ret;
        }
        public async Task<PodcastEntry> GetAsync(string userId, string entryId) {
            var ret = await GetAll()
                .Where(e => e.Id == Guid.Parse(entryId) && e.Podcast.AppUser.Id == userId)
                .Include(e => e.Podcast)
                .Include(e => e.Podcast.AppUser)
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
        public async Task<PodcastEntry> GetFeaturedEpisode(Podcast podcast) {
            return await GetContext()
                .PodcastEntries
                .OrderByDescending(e => e.UpdateDate)
                .FirstOrDefaultAsync(e => e.Podcast == podcast);
        }
    }
}
