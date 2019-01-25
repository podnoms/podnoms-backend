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
    }

}