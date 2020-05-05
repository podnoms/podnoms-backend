using PodNoms.Data.Enums;

namespace PodNoms.Data.Interfaces {
    public interface ICachedEntity {
        string GetCacheKey(CacheType type);
    }
}
