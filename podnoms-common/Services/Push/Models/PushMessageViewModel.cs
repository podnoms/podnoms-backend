using Lib.Net.Http.WebPush;

namespace PodNoms.Common.Services.Push.Models {
    public class PushMessageViewModel {
        public string Topic { get; set; }

        public string Notification { get; set; }
        public string Target { get; set; }
        public PushMessageUrgency Urgency { get; set; } = PushMessageUrgency.Normal;
    }
}
