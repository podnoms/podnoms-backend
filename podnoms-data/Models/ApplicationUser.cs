using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using PodNoms.Data.Annotations;
using PodNoms.Data.Interfaces;

namespace PodNoms.Data.Models {
    [Flags]
    public enum NotificationOptions {
        NewPlaylistEpisode = 1,
        UploadCompleted = 2,
        RecommendedPodcasts = 4,
        StorageExceeded = 8,
        PlaylistEntryCountExceeded = 16
    }

    //TODO: Perhaps this shouldn't be a slug, it's the most visible slug in the application
    //TODO: And it causes confusion for users as it isn't an everyday term
    //TODO: It's really just a unique username
    public class ApplicationUser : IdentityUser, ISluggedEntity {
        // Extended Properties
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public long? FacebookId { get; set; }
        public string TwitterHandle { get; set; }

        public string PictureUrl { get; set; }

        public long? DiskQuota { get; set; }

        [SlugField(sourceField: "FullName")]
        public string Slug { get; set; }
        public string FullName => $"{FirstName} {LastName}";

        public virtual List<AccountSubscription> AccountSubscriptions { get; set; } = new List<AccountSubscription>();
        public virtual List<Donation> Donations { get; set; } = new List<Donation>();
        public virtual List<Podcast> Podcasts { get; set; }
        public virtual List<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
        public DateTime LastSeen { get; set; }
        public string IpAddress { get; set; }
        public string CountryCode { get; set; }
        public string CountryName { get; set; }
        public string RegionCode { get; set; }
        public string RegionName { get; set; }
        public string City { get; set; }
        public string Zip { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        public PatreonToken PatreonToken { get; set; }

        public NotificationOptions EmailNotificationOptions { get; set; }

        public virtual List<IssuedApiKey> IssuedApiKeys { get; set; } = new List<IssuedApiKey>();

        public int? PlaylistAllowedEntryCount { get; set; }

        public void AddRefreshToken(string token, string remoteIpAddress, double daysToExpire = 5) {
            RefreshTokens.Add(new RefreshToken(token, DateTime.UtcNow.AddDays(daysToExpire), this, remoteIpAddress));
        }

        public void RemoveRefreshToken(string refreshToken) {
            RefreshTokens.Remove(RefreshTokens.First(t => t.Token == refreshToken));
        }

        public string GetBestGuessName() {
            if (!string.IsNullOrEmpty(FullName?.Trim())) {
                return FullName;
            }

            if (!string.IsNullOrEmpty(FirstName?.Trim()) || !string.IsNullOrEmpty(LastName?.Trim())) {
                return $"{FirstName} {LastName}".Trim();
            }

            return Email.Split('@')[0];
        }

        public string GetImageUrl(string cdnUrl, string containerName) =>
            GetImageUrl(cdnUrl, containerName, "jpg");

        public string GetImageUrl(string cdnUrl, string containerName, string extension) =>
            PictureUrl.StartsWith("http") && !PictureUrl.Contains("cdn.podnoms.com")
                ? //TODO: <-- this is temporary
                PictureUrl
                : $"{cdnUrl}/{containerName}/profile/{Id}.{extension}?width=725&height=748";

        public string GetThumbnailUrl(string cdnUrl, string containerName) =>
            GetThumbnailUrl(cdnUrl, containerName, "jpg");

        public string GetThumbnailUrl(string cdnUrl, string containerName, string extension) =>
            PictureUrl.StartsWith("http") && !PictureUrl.Contains("cdn.podnoms.com")
                ? //TODO: <-- this is temporary
                PictureUrl
                : $"{cdnUrl}/{containerName}/profile/{Id}.{extension}?width=64&height=64";

        public AccountSubscription GetCurrentSubscription() {
            return this.AccountSubscriptions
                .FirstOrDefault(r =>
                    DateTime.Now >= r.StartDate &&
                    DateTime.Now <= r.EndDate);
        }
    }
}
