namespace PodNoms.AudioParsing.Models {
    public class ProcessingProgress {
        public ProcessingProgress(object payload) {
            Payload = payload;
        }

        public string ProcessingStatus { get; set; }
        public string Progress { get; set; }
        public object Payload { get; set; }
    }

    public class TransferProgress {
        public double Percentage { get; set; }
        public string TotalSize { get; set; }
    }
}
