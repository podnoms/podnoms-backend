using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Extensions.Options;
using PodNoms.Api.Models.Annotations;
using PodNoms.Api.Services.Auth;

namespace PodNoms.Api.Models {
    public class Podcast : BaseEntity, ISluggedEntity {
        public string AppUserId { get; set; }
        public ApplicationUser AppUser { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        [SlugField(sourceField: "Title")]
        public string Slug { get; set; }
        public List<PodcastEntry> PodcastEntries { get; set; }
        public Podcast() {
            PodcastEntries = new List<PodcastEntry>();
        }

        public string GetImageUrl(string cdnUrl, string containerName) {
            return $"{cdnUrl}{containerName}/{this.Id.ToString()}.png";
        }
        public string GetThumbnailUrl(string cdnUrl, string containerName) {
            return $"{cdnUrl}{containerName}/{this.Id.ToString()}-32x32.png";
        }
    }
}
