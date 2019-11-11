using System;
using System.Collections.Generic;

namespace PodNoms.Common.Data.ViewModels.Resources {
    public class UserActivityViewModel {
        public UserActivityViewModel() { }

        public string Name { get; internal set; }
        public string Slug { get; internal set; }
        public string Email { get; internal set; }
        public DateTime LastSeen { get; set; }
        public string IpAddress { get; set; }
        public string CountryCode { get; set; }
        public string CountryName { get; set; }
        public string RegionCode { get; set; }
        public string RegionName { get; set; }
        public string City { get; set; }
        public List<PodcastViewModel> Podcasts { get; internal set; }

        public int PodcastCount { get; set; }
        public int EntryCount { get; set; }
        public bool IsAdmin { get; set; }
    }
}
