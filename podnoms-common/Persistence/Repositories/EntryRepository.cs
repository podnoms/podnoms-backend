using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PodNoms.Common.Data.ViewModels.Resources;
using PodNoms.Common.Persistence.Extensions;
using PodNoms.Common.Utils;
using PodNoms.Data.Models;

namespace PodNoms.Common.Persistence.Repositories {
    public interface IEntryRepository : IRepository<PodcastEntry> {
        Task<PodcastEntry> GetAsync(string userId, string entryId);
        Task<IEnumerable<PodcastEntry>> GetAllForSlugAsync(string podcastSlug);
        Task<IEnumerable<PodcastEntry>> GetAllForUserAsync(string userId);
        Task<PodcastEntry> GetFeaturedEpisode(Podcast podcast);
        Task<List<PodcastEntry>> GetAllButFeatured(Podcast podcast);
        Task<PodcastEntry> GetForUserAndPodcast(string user, string podcast, string entry);
        Task LoadPodcastAsync(PodcastEntry entry);
        Task<PodcastEntrySharingLink> CreateNewSharingLink(SharingViewModel model);
        Task<string> GetIdForShareLink(string sharingId);
        Task<List<PodcastEntry>> GetMissingWaveforms();
        Task<PodcastEntry> GetEntryForShareId(string sharingId);
        Task<IEnumerable<PodcastEntry>> GetRandomPlaylistItems(int amount = 50);
        Task<PodcastEntry> AddOrUpdateWithTags(PodcastEntry entity);
    }

    public class EntryRepository : GenericRepository<PodcastEntry>, IEntryRepository {
        private readonly ITagRepository _tagRepository;

        public EntryRepository(PodNomsDbContext context, ILogger<EntryRepository> logger, ITagRepository tagRepository)
            : base(context, logger) {
            _tagRepository = tagRepository;
        }

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
                .OrderByDescending(e => e.CreateDate)
                .FirstOrDefaultAsync(e => e.Podcast == podcast);
        }

        public async Task<List<PodcastEntry>> GetAllButFeatured(Podcast podcast) {
            return await GetContext()
                .PodcastEntries
                .OrderByDescending(e => e.CreateDate)
                .Where(e => e.Podcast == podcast)
                .Skip(1)
                .ToListAsync();
        }

        public async Task<PodcastEntry> GetForUserAndPodcast(string user, string podcast, string entry) {
            return await GetContext()
                .PodcastEntries
                .Include(p => p.Podcast)
                .Include(p => p.Podcast.AppUser)
                .Where(r => r.Podcast.AppUser.Slug == user)
                .Where(r => r.Podcast.Slug == podcast)
                .Where(r => r.Slug == entry)
                .FirstOrDefaultAsync();
        }

        public async Task<List<PodcastEntry>> GetMissingWaveforms() {
            var qry = GetContext()
                .PodcastEntries
                .Where(r => r.WaveformGenerated == false)
                .Where(r => r.CreateDate <= System.DateTime.Now.AddMinutes(-10));

            // var sql = qry.ToSql();
            // Console.WriteLine(sql);

            return await qry.ToListAsync();
        }

        /// <summary>
        /// Base36 encode the model's ID with extra parity bit
        /// </summary>
        /// <param name="model">The incoming model to convert</param>
        /// <returns>Base36 encoded string</returns>
        public async Task<PodcastEntrySharingLink> CreateNewSharingLink(SharingViewModel model) {
            var entry = await GetAsync(model.Id);
            if (entry is null) return null;

            var token = TokenGenerator.GenerateToken();
            while (await GetContext()
                .PodcastEntrySharingLinks
                .Where(x => x.LinkId == token)
                .AsNoTracking()
                .FirstOrDefaultAsync() != null) {
                token = TokenGenerator.GenerateToken();
            }

            ;
            var link = new PodcastEntrySharingLink {
                LinkId = token,
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
                .Include(e => e.Podcast)
                .Include(e => e.Podcast.AppUser)
                .Where(e => e.SharingLinks.Any(l => l.LinkId == sharingId))
                .FirstOrDefaultAsync();
            return entry;
        }

        public async Task<IEnumerable<PodcastEntry>> GetRandomPlaylistItems(int amount = 50) {
            var results = await GetAll()
                .Where(e => e.Podcast.Private == false)
                .Where(e => e.Podcast.AppUser.Slug == "fergal-moran")
                .OrderBy(e => System.Guid.NewGuid())
                .Take(amount)
                .ToListAsync();
            return results;
        }

        public async Task<PodcastEntry> AddOrUpdateWithTags(PodcastEntry entity) {
            entity.Tags = await _tagRepository.UpdateAndMerge(entity.Tags);
            return base.AddOrUpdate(entity);
        }
    }
}
