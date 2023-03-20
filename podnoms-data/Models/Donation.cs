namespace PodNoms.Data.Models {
    public class Donation : BaseEntity {
        public virtual ApplicationUser AppUser { get; set; }
        public long Amount { get; set; }
    }
}
