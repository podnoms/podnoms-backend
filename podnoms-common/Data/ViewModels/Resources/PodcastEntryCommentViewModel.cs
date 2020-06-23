using System;

namespace PodNoms.Common.Data.ViewModels.Resources {
    public class PodcastEntryCommentViewModel {

        public string FromName { get; set; }
        public string FromEmail { get; set; }
        public string Comment { get; set; }
        public string AvatarImage { get; set; }
        public bool IsSpam { get; set; } = false;
        public DateTime? CommentDate { get; set; }
        public int? Timestamp { get; set; }
    }
}
