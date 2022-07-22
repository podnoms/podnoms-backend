﻿using System;
using System.Collections.Generic;

namespace PodNoms.Common.Data.ViewModels.Resources {
    public class ProfileViewModel {
        public string Id { get; set; }
        public string Slug { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Description { get; set; }
        public string TwitterHandle { get; set; }
        public string ProfileImageUrl { get; set; }
        public string ThumbnailImageUrl { get; set; }
        public string ApiKey { get; set; }
        public int EmailNotificationOptions { get; set; }
        public DateTime? SubscriptionValidUntil { get; set; }
        public bool IsFluent { get; set; }
        public List<string> Roles { get; set; }
        public SubscriptionViewModel Subscription { get; set; }

        public int PodcastCount { get; set; }
        public int EpisodeCount { get; set; }
    }
}
