namespace PodNoms.Data.Models {
    public enum RequestType {
        Support,
        FlagParseUrl
    }
    public class UserRequest : BaseEntity {
        public virtual ApplicationUser FromUser { get; set; }
        public string RequestText { get; set; }
        public RequestType RequestType { get; set; }
        
    }
}
