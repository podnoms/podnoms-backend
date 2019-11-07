using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace PodNoms.Common.Data.ViewModels.Resources {
    public class ProfileViewModel {
        public string Id { get; set; }
        public string Slug { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Description { get; set; }
        public string ProfileImageUrl { get; set; }
        public string ThumbnailImageUrl { get; set; }
        public string ApiKey { get; set; }
        public int EmailNotificationOptions { get; set; }
        public bool HasSubscribed { get; set; }
        public string SubscriptionType { get; set; }
        public bool SubscriptionValid { get; set; }
        public DateTime? SubscriptionValidUntil { get; set; }
        public List<string> Roles { get; set; }
    }
}
