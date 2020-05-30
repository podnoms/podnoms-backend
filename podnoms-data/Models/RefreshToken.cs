using System;

namespace PodNoms.Data.Models {
    public class RefreshToken : BaseEntity {
        public RefreshToken() {
        }
        public RefreshToken(string token, DateTime expires, ApplicationUser user, string remoteIpAddress) {
            Token = token;
            Expires = expires;
            AppUser = user;
            RemoteIpAddress = remoteIpAddress;
        }
        public string Token { get; private set; }
        public DateTime Expires { get; private set; }

        public string AppUserId { get; set; }
        public ApplicationUser AppUser { get; set; }

        public bool Active => DateTime.UtcNow <= Expires;

        public string RemoteIpAddress { get; private set; }
    }
}
