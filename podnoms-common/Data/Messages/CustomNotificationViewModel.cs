using System;

namespace PodNoms.Common.Data.Messages {
    public sealed class CustomNotificationMessage {
        public Guid PodcastId { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public string Url { get; set; }
    }
}
