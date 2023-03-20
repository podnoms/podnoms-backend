namespace PodNoms.Data.Models {
    public class ApplicationUserSlugRedirects : BaseEntity {

        public string ApplicationUserId { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }

        public string OldSlug { get; set; }
    }
}
