namespace PodNoms.Data.Models {
    public class PatreonToken : BaseEntity {
        public string AccessToken { get; set; }

        public long ExpiresIn { get; set; }

        public string TokenType { get; set; }

        public string FullName { get; set; }

        public string RefreshToken { get; set; }

        public string Version { get; set; }

        public string AppUserId { get; set; }
        public virtual ApplicationUser AppUser { get; set; }
    }
}
