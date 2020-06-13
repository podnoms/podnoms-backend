namespace PodNoms.Data.Models {
    public class UserRequest : BaseEntity {
        public virtual ApplicationUser FromUser { get; set; }
        public string RequestText { get; set; }
    }
}
