using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using Flurl;
using PodNoms.Data.Annotations;
using PodNoms.Data.Enums;
using PodNoms.Data.Interfaces;
using PodNoms.Data.Models.Notifications;

namespace PodNoms.Data.Models;

public class PodcastAggregator : BaseEntity {
  public string Name { get; set; }
  [MaxLength(2000)] public string Url { get; set; }
  [MaxLength(2000)] public string ImageUrl { get; set; }
  [JsonIgnore] public virtual Podcast Podcast { get; set; }
}

public class Podcast : BaseEntity, ISluggedEntity, ICachedEntity {
  public string AppUserId { get; set; }
  public virtual ApplicationUser AppUser { get; set; }
  public string Title { get; set; }
  public string Description { get; set; }

  public string CustomDomain { get; set; }
  public string CustomRssDomain { get; set; }
  public virtual List<PodcastEntry> PodcastEntries { get; set; } = new();
  public virtual Category Category { get; set; }
  public virtual List<Subcategory> Subcategories { get; set; }
  public virtual List<Notification> Notifications { get; set; }

  public string PublicTitle { get; set; }
  [MaxLength(2000)] public string FacebookUrl { get; set; }
  [MaxLength(2000)] public string TwitterUrl { get; set; }

  public string GoogleAnalyticsTrackingId { get; set; }

  public virtual List<PodcastAggregator> Aggregators { get; set; } = new();

  public string GetCacheKey(CacheType type) {
    return $"podcast|{AppUser.Slug}|{Slug}|{type.ToString()}";
  }

  [SlugField("Title")] public string Slug { get; set; }

  public string GetRawImageUrl(string cdnUrl, string containerName) {
    return Url.Combine(cdnUrl, containerName, $"podcast/{Id}.jpg");
  }

  public string GetRssUrl(string rssUrl) {
    return Url.Combine(rssUrl, AppUser.Slug, Slug);
  }

  public string GetCoverImageUrl(string cdnUrl, string containerName) {
    return Url.Combine(cdnUrl, containerName, $"podcast/{Id}.jpg?width=1920&height=1080&rmode=stretch");
  }

  public string GetImageUrl(string cdnUrl, string containerName) {
    return Url.Combine(cdnUrl, containerName, $"podcast/{Id}.jpg?width=725&height=748");
  }

  public string GetThumbnailUrl(string cdnUrl, string containerName) {
    return Url.Combine(cdnUrl, containerName, $"podcast/{Id}.jpg?width=32&height=32");
  }

  public string GetAuthenticatedUrl(string siteUrl) {
    return Url.Combine(siteUrl, $"/podcasts/{Slug}");
  }

  public string GetPagesUrl(string siteUrl) {
    return Url.Combine(siteUrl, AppUser.Slug, Slug);
  }

  public DateTime GetLastEntryDate() {
    return PodcastEntries
      .OrderByDescending(e => e.UpdateDate)
      .Select(r => r.UpdateDate)
      .FirstOrDefault();
  }

  #region AuthStuff

  public bool Private { get; set; } = false;
  public string AuthUserName { get; set; }
  public byte[] AuthPassword { get; set; }
  public byte[] AuthPasswordSalt { get; set; }

  #endregion
}
