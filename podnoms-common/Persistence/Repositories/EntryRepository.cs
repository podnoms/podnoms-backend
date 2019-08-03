using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PodNoms.Common.Data.ViewModels.Resources;
using PodNoms.Data.Models;

namespace PodNoms.Common.Persistence.Repositories {
    public interface IEntryRepository : IRepository<PodcastEntry> {
        Task<PodcastEntry> GetAsync(string userId, string entryId);
        Task<IEnumerable<PodcastEntry>> GetAllForSlugAsync(string podcastSlug);
        Task<IEnumerable<PodcastEntry>> GetAllForUserAsync(string userId);
        Task<PodcastEntry> GetFeaturedEpisode(Podcast podcast);
        Task LoadPodcastAsync(PodcastEntry entry);
        Task<PodcastEntrySharingLink> CreateNewSharingLink(SharingViewModel model);
        Task<string> GetIdForShareLink(string sharingId);
        Task<List<PodcastEntry>> GetMissingWaveforms();
        Task<PodcastEntry> GetEntryForShareId(string sharingId);
    }

    public class EntryRepository : GenericRepository<PodcastEntry>, IEntryRepository {
        public EntryRepository(PodNomsDbContext context, ILogger<EntryRepository> logger) : base(context, logger) { }

        public new async Task<PodcastEntry> GetAsync(string id) {
            return await GetAsync(Guid.Parse(id));
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
        public async Task<List<PodcastEntry>> GetMissingWaveforms() {
            return await GetContext()
                .PodcastEntries.Where(r => r.WaveformGenerated == false)
                .Where(r => r.Id == Guid.Parse("6336bfb8-5279-4864-5245-08d6d7d01a5e"))
                .ToListAsync();
        }
        /// <summary>
        /// Base36 encode the model's ID with extra parity bit
        /// </summary>
        /// <param name="model">The incoming model to convert</param>
        /// <returns>Base36 encoded string</returns>
        public async Task<PodcastEntrySharingLink> CreateNewSharingLink(SharingViewModel model) {
            char[] padding = { '=' };
            var ret = string.Empty;
            var entry = await GetAsync(model.Id);
            if (entry is null) return null;
            var index = await GetContext()
                .PodcastEntrySharingLinks
                .OrderByDescending(l => l.LinkIndex)
                .Select(l => l.LinkIndex + 1)
                .FirstOrDefaultAsync();

            ret = System.Convert.ToBase64String(BitConverter.GetBytes(index))
                .TrimEnd(padding).Replace('+', '-').Replace('/', '_');
            var link = new PodcastEntrySharingLink {
                LinkId = ret,
                ValidFrom = model.ValidFrom,
                ValidTo = model.ValidTo
            };
            if (entry.SharingLinks is null)
                entry.SharingLinks = new List<PodcastEntrySharingLink>();
            entry.SharingLinks.Add(link);
            return link;
        }

        public async Task<string> GetIdForShareLink(string sharingId) {
            var entry = await GetAll()
                .Where(e => e.SharingLinks.Any(l => l.LinkId == sharingId))
                .Select(e => e.Id.ToString())
                .FirstOrDefaultAsync();
            return entry;
        }

        public async Task<PodcastEntry> GetEntryForShareId(string sharingId) {
            var entry = await GetAll()
                .Where(e => e.SharingLinks.Any(l => l.LinkId == sharingId))
                .FirstOrDefaultAsync();
            return entry;
        }
    }
}
