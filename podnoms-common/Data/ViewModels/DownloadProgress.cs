using PodNoms.Data.Enums;
using PodNoms.Data.Models;

namespace PodNoms.Common.Data.ViewModels {
    public class ProcessingProgress {
        public ProcessingProgress(object payload) {
            Payload = payload;
        }
        
        public ProcessingStatus ProcessingStatus { get; set; }
        public string Progress { get; set; }
        public object Payload { get; set; }
    }
    public class TransferProgress {
        public double Percentage { get; set; }
        public string TotalSize { get; set; }
    }
}
