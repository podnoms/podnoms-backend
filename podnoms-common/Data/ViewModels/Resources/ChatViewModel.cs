using System;

namespace PodNoms.Common.Data.ViewModels.Resources {
    public class ChatViewModel {
        public string MessageId { get; set; }
        public string Message { get; set; }
        public string FromUserImage { get; set; }
        public string FromUserId { get; set; }
        public string FromUserName { get; set; }
        public string ToUserId { get; set; }
        public string ToUserName { get; set; }
        public DateTime MessageDate { get; set; }
    }
}
