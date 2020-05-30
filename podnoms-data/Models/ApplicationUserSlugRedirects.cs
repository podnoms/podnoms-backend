using System;
using PodNoms.Data.Interfaces;

namespace PodNoms.Data.Models {
    public class ApplicationUserSlugRedirects : IEntity {
        public Guid Id { get; set; }

        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }

        public string OldSlug { get; set; }

        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
    }
}
