namespace PodNoms.Common.Data.Settings {
    public class StorageSettings {
        public string ConnectionString { get; set; }
        public string CdnUrl { get; set; }
        public string ImageUrl { get; set; }
        public long DefaultUserQuota { get; set; }
        public int DefaultEntryCount { get; set; }
    }
}
