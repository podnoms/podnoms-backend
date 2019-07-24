using System;
using System.Collections.Generic;
using System.Text;

namespace PodNoms.Common.Messaging.Contracts {
    public class NotifyUserMessage {
        public string UserId { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public string Target { get; set; }
        public string Image { get; set; }
    }
}
