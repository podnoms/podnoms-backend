using System;

namespace PodNoms.Common.Data.ViewModels.Resources {
    public class SharingViewModel {
        public string Id { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
    }
}