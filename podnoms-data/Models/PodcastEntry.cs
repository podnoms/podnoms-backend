using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Flurl;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using PodNoms.Data.Annotations;
using PodNoms.Data.Enums;
using PodNoms.Data.Interfaces;
using PodNoms.Data.ViewModels;

namespace PodNoms.Data.Models;

public enum ShareOptions {
  Public = 1 << 0,
  Private = 1 << 1,
  Download = 1 << 2
}

public class PodcastEntry : BaseEntity, ISluggedEntity, ICachedEntity, IHubNotifyEntity {
  public string Author { get; set; }
  public string Title { get; set; }

  public string Description { get; set; }
  [MaxLength(2000)] public string SourceUrl { get; set; }

  [MaxLength(2000)] public string AudioUrl { get; set; }
  public double AudioLength { get; set; }

  public long AudioFileSize { get; set; }

  [MaxLength(2000)] public string ImageUrl { get; set; }
  public string ProcessingPayload { get; set; }
  public ProcessingStatus ProcessingStatus { get; set; } = ProcessingStatus.Accepted;
  public DateTime? SourceCreateDate { get; set; }
  public string SourceItemId { get; set; }

  public int MetadataStatus { get; set; } = 0;

  public int ShareOptions { get; set; }
  public bool Processed { get; set; }
  public bool WaveformGenerated { get; set; }
  public Guid PodcastId { get; set; }

  [JsonIgnore] public Guid? PlaylistId { get; set; }
  [JsonIgnore] public virtual Playlist Playlist { get; set; }

  [JsonIgnore] public virtual Podcast Podcast { get; set; }

  [JsonIgnore] public virtual List<PodcastEntrySharingLink> SharingLinks { get; set; }

  [JsonIgnore] public virtual List<ActivityLogPodcastEntry> ActivityLogs { get; set; }


  public virtual List<EntryComment> Comments { get; set; } = new();
  public virtual ICollection<EntryTag> Tags { get; set; } = new List<EntryTag>();

  private string extension => "jpg";

  public string GetCacheKey(CacheType type) {
    return Podcast?.GetCacheKey(type);
  }

  public string GetHubMethodName() {
    return "podcast-entry-added";
  }

  public RealtimeEntityUpdateMessage SerialiseForHub() {
    return new RealtimeEntityUpdateMessage() {
      Channel = GetHubMethodName(),
      Id = Id.ToString(),
      Title = Title
    };
  }

  public string UserIdForRealtime(DbContext context) {
    try {
      context.Entry(this).Reference(p => p.Podcast).Load();
      context.Entry(Podcast).Reference(p => p.AppUser).Load();
      return Podcast?.AppUserId;
    } catch (Exception) {
      return string.Empty;
    }
  }

  [SlugField("Title")] public string Slug { get; set; }

  public override string ToString() {
    return $"PodcastEntry: {Id}: {Slug} -- {Podcast?.Slug} -- {Podcast?.AppUser?.Slug}";
  }

  public string GetDownloadUrl(string downloadUrlRoot) {
    return $"{downloadUrlRoot}/{Id}";
  }

  public string GetPcmUrl(string cdnUrl, string containerName) {
    return $"{cdnUrl}/{containerName}/{Id}.json";
  }

  public string GetAudioUrl(string audioUrl) {
    return GetAudioUrl(audioUrl, "mp3");
  }

  public string GetAudioUrl(string audioUrl, string extension) {
    return $"{audioUrl}/{Id}.{extension}";
  }

  public string GetRssAudioUrl(string audioUrl) {
    return GetAudioUrl(audioUrl, "mp3");
  }

  public string GetRawAudioUrl(string cdnUrl, string containerName, string extension) {
    return Url.Combine(cdnUrl, containerName, $"{Id}.{extension}");
  }

  public string GetImageUrl(string cdnUrl, string containerName) {
    return ImageUrl.StartsWith("http")
      ? ImageUrl
      : Url.Combine(cdnUrl, containerName,
        $"entry/{Id}.{extension}?width=725&height=748&cb={Guid.NewGuid()}");
  }

  public string GetThumbnailUrl(string cdnUrl, string containerName) {
    return ImageUrl.StartsWith("http")
      ? ImageUrl
      : Url.Combine(cdnUrl, containerName,
        $"entry/{Id}.{extension}?width=64&height=64&cb={Guid.NewGuid()}");
  }

  public string GetInternalStorageUrl(string cdnUrl) {
    return $"{cdnUrl}/{AudioUrl}";
  }

  public string GetFileDownloadName() {
    return $"{Title}.mp3";
  }

  public string GetPagesUrl(string pagesUrl) {
    return Url.Combine(pagesUrl, Podcast.AppUser.Slug, Podcast.Slug, Slug);
  }

  public string GetDebugString(CacheType type) {
    return Podcast?.GetCacheKey(type);
  }
}
