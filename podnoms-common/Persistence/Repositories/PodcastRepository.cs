﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PodNoms.Common.Auth;
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
        Task<List<PodcastAggregator>> GetAggregators(Guid podcastId);
        Task<string> GetActivePodcast(string userId);
    }

    public class PodcastRepository : GenericRepository<Podcast>, IPodcastRepository {
        public PodcastRepository(PodNomsDbContext context, ILogger<PodcastRepository> logger) :
            base(context, logger) { }

        public async Task<Podcast> GetAsync(string userId, string id) {
            return await GetAsync(userId, Guid.Parse(id));
        }

        public async Task<Podcast> GetAsync(string userId, Guid id) {
            var ret = await GetAll()
                .Where(p => p.Id == id && p.AppUser.Id == userId)
                .Include(p => p.PodcastEntries)
                .Include(p => p.AppUser)
                .Include(p => p.Category)
                .Include(p => p.Subcategories)
                .Include(p => p.Notifications)
                .FirstOrDefaultAsync();
            ret.PodcastEntries = ret.PodcastEntries.OrderByDescending(r => r.CreateDate).ToList();
            return ret;
        }

        public new async Task<Podcast> GetAsync(Guid podcastId) {
            var ret = await GetAll()
                .Where(p => p.Id == podcastId)
                .Include(p => p.PodcastEntries)
                .Include(p => p.AppUser)
                .Include(p => p.Category)
                .Include(p => p.Subcategories)
                .Include(p => p.Notifications)
                .FirstOrDefaultAsync();
            ret.PodcastEntries = ret.PodcastEntries.OrderByDescending(r => r.CreateDate).ToList();
            return ret;
        }

        public async Task<Podcast> GetForUserAndSlugAsync(Guid userId, string podcastSlug) {
            var ret = await GetAll()
                .Where(r => r.AppUser.Id == userId.ToString() && r.Slug == podcastSlug)
                .Include(p => p.AppUser)
                .Include(p => p.PodcastEntries)
                .Include(p => p.Category)
                .Include(p => p.Subcategories)
                .Include(p => p.Notifications)
                .FirstOrDefaultAsync();
            if (ret != null && ret.PodcastEntries != null) {
                ret.PodcastEntries = ret.PodcastEntries.OrderByDescending(r => r.CreateDate).ToList();
            }
            return ret;
        }
        public async Task<Podcast> GetForUserAndSlugAsync(string userSlug, string podcastSlug) {
            var ret = await GetAll()
                .Where(r => r.AppUser.Slug == userSlug && r.Slug == podcastSlug)
                .Include(p => p.AppUser)
                .Include(p => p.PodcastEntries)
                .Include(p => p.Category)
                .Include(p => p.Subcategories)
                .Include(p => p.Notifications)
                .FirstOrDefaultAsync();
            if (ret != null && ret.PodcastEntries != null) {
                ret.PodcastEntries = ret.PodcastEntries.OrderByDescending(r => r.CreateDate).ToList();
            }
            return ret;
        }

        public async Task<IEnumerable<Podcast>> GetAllForUserAsync(string userId) {
            var ret = GetAll()
                .Where(u => u.AppUser.Id == userId)
                .Include(p => p.AppUser)
                .Include(p => p.PodcastEntries)
                .Include(p => p.Category)
                .Include(p => p.Subcategories)
                .Include(p => p.Notifications);
            return await ret.ToListAsync();
        }
        public async Task<string> GetActivePodcast(string userId) {
            var podcast = await GetAll().Where(p => p.AppUser.Id == userId)
                .OrderByDescending(p => p.UpdateDate)
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
            if (podcast.AuthPassword != null && podcast.AuthPassword.Length != 0 && (podcast.Private)) {
                var salt = PBKDFGenerators.GenerateSalt();
                var password = PBKDFGenerators.GenerateHash(podcast.AuthPassword, salt);

                podcast.AuthPasswordSalt = salt;
                podcast.AuthPassword = password;
            }

            return base.Update(podcast);
        }
    }
}
