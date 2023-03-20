using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PodNoms.Common.Utils.Crypt;
using PodNoms.Data.Models;

namespace PodNoms.Common.Persistence.Repositories {
    public interface IPodcastRepository : IRepository<Podcast> {
        Task<Podcast> GetAsync(string userId, string id);
        Task<Podcast> GetAsync(string userId, Guid id);
        new Task<Podcast> GetAsync(Guid id);
        Task<IEnumerable<Podcast>> GetAllForUserAsync(string userId);
        Task<Podcast> GetForUserAndSlugAsync(Guid userId, string podcastSlug);
        Task<Podcast> GetForUserAndSlugAsync(string userSlug, string podcastSlug);
        Task<Podcast> GetRandomForUser(string userId);
        Task<List<PodcastAggregator>> GetAggregators(Guid podcastId);
        Task<string> GetActivePodcast(string userId);
    }

    internal class PodcastRepository : GenericRepository<Podcast>, IPodcastRepository {
        public PodcastRepository(PodNomsDbContext context, ILogger<PodcastRepository> logger) :
            base(context, logger) {
        }

        public async Task<Podcast> GetAsync(string userId, string id) {
            return await GetAsync(userId, Guid.Parse(id));
        }

        public async Task<Podcast> GetAsync(string userId, Guid id) {
            var ret = await GetAll()
                .AsNoTracking()
                .Where(p => p.Id == id && p.AppUser.Id == userId)
                .Include(p => p.AppUser)
                .AsNoTracking()
                .FirstOrDefaultAsync();
            return ret;
        }

        public new async Task<Podcast> GetAsync(Guid podcastId) {
            var ret = await GetAll()
                .Where(p => p.Id == podcastId)
                .Include(p => p.AppUser)
                .AsNoTracking()
                .FirstOrDefaultAsync();
            return ret;
        }

        public async Task<Podcast> GetRandomForUser(string userId) {
            return await GetAll()
                .Where(r => r.AppUser.Id == userId)
                .OrderBy(r => System.Guid.NewGuid().ToString())
                .Take(1)
                .SingleOrDefaultAsync();
        }

        public async Task<Podcast> GetForUserAndSlugAsync(Guid userId, string podcastSlug) {
            var ret = await GetAll()
                .Where(r => r.AppUser.Id == userId.ToString() && r.Slug == podcastSlug)
                .OrderByDescending(r =>
                    r.PodcastEntries
                        .OrderByDescending(e => e.UpdateDate)
                        .SingleOrDefault().UpdateDate)
                .FirstOrDefaultAsync();
            if (ret?.PodcastEntries != null) {
                ret.PodcastEntries = ret
                    .PodcastEntries
                    .OrderByDescending(r => r.CreateDate)
                    .ToList();
            }

            return ret;
        }

        public async Task<Podcast> GetForUserAndSlugAsync(string userSlug, string podcastSlug) {
            var ret = await GetAll()
                .Where(r => r.AppUser.Slug == userSlug && r.Slug == podcastSlug)
                // .Include(p => p.AppUser)
                // .Include(p => p.PodcastEntries)
                // .Include(p => p.Category)
                // .Include(p => p.Subcategories)
                // .Include(p => p.Notifications)
                .FirstOrDefaultAsync();
            if (ret != null && ret.PodcastEntries != null) {
                ret.PodcastEntries = ret.PodcastEntries.OrderByDescending(r => r.CreateDate).ToList();
            }

            return ret;
        }

        public async Task<IEnumerable<Podcast>> GetAllForUserAsync(string userId) {
            var ret = GetAll()
                .Where(u => u.AppUser.Id == userId)
                .OrderByDescending(p => p.PodcastEntries
                    .OrderByDescending(c => c.CreateDate)
                    .Select(c => c.CreateDate)
                    .FirstOrDefault()
                );
            // .Include(p => p.AppUser)
            // .Include(p => p.PodcastEntries)
            // .Include(p => p.Category)
            // .Include(p => p.Subcategories)
            // .Include(p => p.Notifications);
            return await ret.ToListAsync();
        }

        public async Task<string> GetActivePodcast(string userId) {
            var podcast = await GetAll()
                .Where(p => p.AppUser.Id == userId)
                .OrderByDescending(p => p.PodcastEntries.Max(e => e.UpdateDate))
                .FirstOrDefaultAsync();
            return podcast?.Slug;
        }

        public async Task<List<PodcastAggregator>> GetAggregators(Guid podcastId) {
            return await GetContext()
                .PodcastAggregators
                .Where(p => p.Podcast.Id == podcastId)
                .ToListAsync();
        }

        public new Podcast AddOrUpdate(Podcast podcast) {
            // hash the passwords
            if (podcast.AuthPassword == null || podcast.AuthPassword.Length == 0 || (!podcast.Private)) {
                return base.Update(podcast);
            }

            var salt = PBKDFGenerators.GenerateSalt();
            var password = PBKDFGenerators.GenerateHash(podcast.AuthPassword, salt);

            podcast.AuthPasswordSalt = salt;
            podcast.AuthPassword = password;

            return base.Update(podcast);
        }
    }
}
