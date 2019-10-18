using System.Collections.Generic;

namespace PodNoms.Common.Data.ViewModels.Resources {
    public class NotificationConfigViewModel {
        public string Type { get; set; }
        public List<NotificationOptionViewModel> Options { get; set; }
        // public List<string> Options;
    }
}
