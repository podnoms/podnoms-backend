using System;
using PodNoms.Data.Interfaces;

namespace PodNoms.Data.Models {
    public class ApplicationUserSlugRedirects : BaseEntity {

        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }

        public string OldSlug { get; set; }
    }
}
