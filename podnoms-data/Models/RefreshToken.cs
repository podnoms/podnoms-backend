using System;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

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
        [XmlIgnore]
        public string Token { get; private set; }

        [XmlIgnore]
        public DateTime Expires { get; private set; }

        public string AppUserId { get; set; }
        public virtual ApplicationUser AppUser { get; set; }

        public bool Active => DateTime.UtcNow <= Expires;

        [XmlIgnore]
        public string RemoteIpAddress { get; private set; }
    }
}
