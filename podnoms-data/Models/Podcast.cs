using System;
using System.Linq;
using System.Collections.Generic;
using PodNoms.Data.Annotations;
using PodNoms.Data.Interfaces;
using PodNoms.Data.Models.Notifications;

namespace PodNoms.Data.Models {

    public class Podcast : BaseEntity, ISluggedEntity {
        public Podcast() {
            PodcastEntries = new List<PodcastEntry>();
        }

        public string AppUserId { get; set; }
        public ApplicationUser AppUser { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        [SlugField(sourceField: "Title")] public string Slug { get; set; }

        public string CustomDomain { get; set; }
        public List<PodcastEntry> PodcastEntries { get; set; }
        public Category Category { get; set; }
        public List<Subcategory> Subcategories { get; set; }
        public List<Notification> Notifications { get; set; }

        public string PublicTitle { get; set; }
        public string FacebookUrl { get; set; }
        public string TwitterUrl { get; set; }

        #region AuthStuff

        public bool Private { get; set; } = false;
        public string AuthUserName { get; set; }
        public byte[] AuthPassword { get; set; }
        public byte[] AuthPasswordSalt { get; set; }

        #endregion

        public string GetImageUrl(string cdnUrl, string containerName) =>
            Flurl.Url.Combine(cdnUrl, containerName, $"podcast/{Id}.jpg?width=725&height=748");
        public string GetThumbnailUrl(string cdnUrl, string containerName) =>
            Flurl.Url.Combine(cdnUrl, containerName, $"podcast/{Id}.jpg?width=32&height=32");
        public string GetAuthenticatedUrl(string siteUrl) =>
            Flurl.Url.Combine(siteUrl, $"/podcasts/{Slug}");
        public string GetPagesUrl(string pagesUrl) =>
            Flurl.Url.Combine(pagesUrl, this.AppUser.Slug, this.Slug);

        public DateTime? GetLastEntryDate() {
            var lastEntry = this.PodcastEntries
                .OrderByDescending(e => e.UpdateDate)
                .Select(r => r.UpdateDate)
                .FirstOrDefault();
            return lastEntry == null ? this.UpdateDate : lastEntry;
        }
    }
}
