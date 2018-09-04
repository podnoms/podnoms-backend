using System.Collections.Generic;

namespace PodNoms.Api.Models.ViewModels.Resources {
    public class NotificationConfigViewModel {
        public string Type { get; set; }
        public string ValueType { get; set; }
        public List<NotificationOptionViewModel<string>> Options;
    }
}