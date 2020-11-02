namespace PodNoms.Data.Models {
    public abstract class Comment : BaseEntity {
        public string CommentText { get; set; }
        public bool IsSpam { get; set; }
    }

    public class EntryComment : Comment {
        public string FromUser { get; set; }
        public string FromUserEmail { get; set; }
        public int? Timestamp { get; set; }
        public virtual PodcastEntry PodcastEntry { get; set; }
    }
}
