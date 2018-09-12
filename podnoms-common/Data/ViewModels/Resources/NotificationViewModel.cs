using System;
using System.Collections.Generic;

namespace PodNoms.Data.Models.ViewModels.Resources {
    public class NotificationViewModel {
        public string Id { get; set; }
        public Guid PodcastId { get; set; }
        public string Type { get; set; }
        public List<NotificationOptionViewModel<string>> Options { get; set; }
    }
}