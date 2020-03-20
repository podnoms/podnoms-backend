namespace PodNoms.Common.Services.Caching {
    public class RedisCacheSettings {
        public bool Enabled { get; set; }
        public string ConnectionString { get; set; }
    }
}
