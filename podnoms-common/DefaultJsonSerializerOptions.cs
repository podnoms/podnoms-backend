using System.Text.Json;

namespace PodNoms.Common {
    public static class DefaultJsonSerializerOptions {
        public static JsonSerializerOptions Options => new JsonSerializerOptions {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
            IgnoreNullValues = true
        };
    }
}
