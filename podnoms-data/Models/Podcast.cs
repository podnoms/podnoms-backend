using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using PodNoms.Data.Annotations;
using PodNoms.Data.Interfaces;
using PodNoms.Data.Models.Notifications;
using PodNoms.Data.Enums;

namespace PodNoms.Data.Models {
    public class PodcastAggregator : BaseEntity {
        public string Name { get; set; }
        [MaxLength(2000)] public string Url { get; set; }
        [MaxLength(2000)] public string ImageUrl { get; set; }
        public virtual Podcast Podcast { get; set; }
    }

    public class Podcast : BaseEntity, ISluggedEntity, ICachedEntity {
        public Podcast() {
            PodcastEntries = new List<PodcastEntry>();
            Aggregators = new List<PodcastAggregator>();
        }

        public string AppUserId { get; set; }
        public virtual ApplicationUser AppUser { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        [SlugField(sourceField: "Title")] public string Slug { get; set; }

        public string CustomDomain { get; set; }
        public string CustomRssDomain { get; set; }
        public virtual List<PodcastEntry> PodcastEntries { get; set; }
        public virtual Category Category { get; set; }
        public virtual List<Subcategory> Subcategories { get; set; }
        public virtual List<Notification> Notifications { get; set; }

        public string PublicTitle { get; set; }
        [MaxLength(2000)] public string FacebookUrl { get; set; }
        [MaxLength(2000)] public string TwitterUrl { get; set; }

        public string GoogleAnalyticsTrackingId { get; set; }

        public virtual List<PodcastAggregator> Aggregators { get; set; }

        #region AuthStuff

        public bool Private { get; set; } = false;
        public string AuthUserName { get; set; }
        public byte[] AuthPassword { get; set; }
        public byte[] AuthPasswordSalt { get; set; }

        #endregion

        public string GetRawImageUrl(string cdnUrl, string containerName) =>
            Flurl.Url.Combine(cdnUrl, containerName, $"podcast/{Id}.jpg");

        public string GetRssUrl(string rssUrl) =>
            Flurl.Url.Combine(rssUrl, this.AppUser.Slug, this.Slug);

        public string GetCoverImageUrl(string cdnUrl, string containerName) =>
            Flurl.Url.Combine(cdnUrl, containerName, $"podcast/{Id}.jpg?width=1920&height=1080&rmode=stretch");

        public string GetImageUrl(string cdnUrl, string containerName) =>
            Flurl.Url.Combine(cdnUrl, containerName, $"podcast/{Id}.jpg?width=725&height=748");

        public string GetThumbnailUrl(string cdnUrl, string containerName) =>
            Flurl.Url.Combine(cdnUrl, containerName, $"podcast/{Id}.jpg?width=32&height=32");

        public string GetAuthenticatedUrl(string siteUrl) =>
            Flurl.Url.Combine(siteUrl, $"/podcasts/{Slug}");

        public string GetPagesUrl(string siteUrl) =>
            Flurl.Url.Combine(siteUrl, this.AppUser.Slug, this.Slug);

        public DateTime GetLastEntryDate() {
            return this.PodcastEntries
                .OrderByDescending(e => e.UpdateDate)
                .Select(r => r.UpdateDate)
                .FirstOrDefault();
        }

        public string GetCacheKey(CacheType type) =>
            $"podcast|{this.AppUser.Slug}|{this.Slug}|{type.ToString()}";
    }
}
