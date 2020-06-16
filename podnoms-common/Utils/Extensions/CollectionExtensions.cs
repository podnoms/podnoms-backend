using System.Collections.Generic;
using System.Linq;

namespace PodNoms.Common.Utils.Extensions {
    public static class CollectionExtensions {
        public static string ToJson(this Dictionary<string, string> dict) {
            var entries = dict.Select(d =>
                $"\"{d.Key}\": [{string.Join(",", d.Value)}]");
            return $"{{{string.Join(",", entries)}}}";
        }
        public static string Join(this IEnumerable<string> source, string separator) {
            return string.Join(separator, source);
        }
    }
}
