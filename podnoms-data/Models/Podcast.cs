using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Extensions.Options;
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

        #region AuthStuff

        public bool? Private { get; set; } = false;
        public string AuthUserName { get; set; }
        public byte[] AuthPassword { get; set; }
        public byte[] AuthPasswordSalt { get; set; }

        #endregion

        public string GetImageUrl(string cdnUrl, string containerName) {
            return $"{cdnUrl}{containerName}/podcast/{Id.ToString()}.png";
        }

        public string GetThumbnailUrl(string cdnUrl, string containerName) {
            return $"{cdnUrl}{containerName}/podcast/{Id.ToString()}-32x32.png";
        }

        public string GetAuthenticatedUrl(string siteUrl) {
            return $"{siteUrl}/podcasts/{Slug}";
        }
    }
}