using System;
using System.Collections.Generic;

namespace PodNoms.Api.Models.ViewModels {
    public class NotificationViewModel {
        public string Id { get; set; }
        public Guid PodcastId { get; set; }
        public string Type { get; set; }
        public IList<NotificationOptionViewModel<string>> Options { get; set; }
    }
}