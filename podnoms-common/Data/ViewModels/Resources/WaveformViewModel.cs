namespace PodNoms.Common.Data.ViewModels.Resources {
    public class WaveformViewModel {
        public string PeakData { get; set; }
        public string PeakDataJson { get; set; }
        public string PeakDataUrl { get; set; }
        public string PeakDataJsonUrl { get; set; }
        public int Bits { get; set; } = 8;
        public string PngUrl { get; set; }
    }
}
