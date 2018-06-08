using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PodNoms.Api.Models;
using PodNoms.Api.Utils;

namespace PodNoms.Api.Persistence {
    public interface IPodcastRepository : IRepository<Podcast> {
        Task<Podcast> GetAsync(string userId, string id);
        Task<Podcast> GetAsync(string userId, Guid id);
        new Task<Podcast> GetAsync(Guid id);
        Task<IEnumerable<Podcast>> GetAllForUserAsync(string userId);
    }
    public class PodcastRepository : GenericRepository<Podcast>, IPodcastRepository {
        public PodcastRepository(PodNomsDbContext context, ILogger<PodcastRepository> logger) : base(context, logger) {
        }
        public async Task<Podcast> GetAsync(string userId, string id) {
            return await GetAsync(userId, Guid.Parse(id));
        }
        public async Task<Podcast> GetAsync(string userId, Guid id) {
            var ret = await GetAll()
                .Where(p => p.Id == id && p.AppUser.Id == userId)
                .Include(p => p.PodcastEntries)
                .Include(p => p.AppUser)
                .FirstOrDefaultAsync();
            return ret;
        }
        public new async Task<Podcast> GetAsync(Guid podcastId) {
            var ret = await GetAll()
                .Where(p => p.Id == podcastId)
                .Include(p => p.PodcastEntries)
                .Include(p => p.AppUser)
                .FirstOrDefaultAsync();
            return ret;
        }
        public async Task<IEnumerable<Podcast>> GetAllForUserAsync(string userId) {
            var ret = GetAll()
                .Where(u => u.AppUser.Id == userId)
                .Include(p => p.AppUser)
                .Include(p => p.PodcastEntries)
                .OrderByDescending(p => p.Id);
            return await ret.ToListAsync();
        }
        public new Podcast AddOrUpdate(Podcast podcast) {
            return base.AddOrUpdate(podcast);
        }
    }
}
