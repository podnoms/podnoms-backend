using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using PodNoms.Data.Annotations;
using PodNoms.Data.Interfaces;

namespace PodNoms.Data.Models {
    public class ApplicationUser : IdentityUser, ISluggedEntity {
        // Extended Properties
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public long? FacebookId { get; set; }
        public string PictureUrl { get; set; }

        public long? DiskQuota { get; set; }

        [SlugField(sourceField: "FullName")] public string Slug { get; set; }
        public string FullName => $"{FirstName} {LastName}";

        public List<AccountSubscription> AccountSubscriptions { get; set; }
        public List<Donation> Donations { get; set; }
        public List<Podcast> Podcasts { get; set; }
        public bool IsAdmin { get; set; } = false;

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

    }
}
