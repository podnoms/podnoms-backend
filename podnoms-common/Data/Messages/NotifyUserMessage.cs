namespace PodNoms.Common.Data.Messages {
    public sealed class NotifyUserMessage {
        public string UserId { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public string Target { get; set; }
        public string Image { get; set; }
    }
}
