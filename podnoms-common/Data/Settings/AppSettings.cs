namespace PodNoms.Data.Models.Settings {
    public class AppSettings {
        public string Version { get; set; }
        public string SiteUrl { get; set; }
        public string CanonicalRssUrl { get; set; }
        public string RssUrl { get; set; }
        public string GoogleApiKey { get; set; }
        public string Downloader { get; set; }
    }
}