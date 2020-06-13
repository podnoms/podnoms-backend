using System.Collections.Generic;
using System.Linq;

namespace PodNoms.Common.Utils.Extensions {
    public static class DictionaryExtensions {

        // Works in C#3/VS2008:
        // Returns a new dictionary of this ... others merged leftward.
        // Keeps the type of 'this', which must be default-instantiable.
        // Example: 
        //   result = map.MergeLeft(other1, other2, ...)
        public static Dictionary<K, V> MergeLeft<K, V>(this Dictionary<K, V> me, params IDictionary<K, V>[] others) {
            var newMap = new Dictionary<K, V>(me, me.Comparer);
            foreach (IDictionary<K, V> src in
                (new List<IDictionary<K, V>> { me }).Concat(others)) {
                // ^-- echk. Not quite there type-system.
                foreach (KeyValuePair<K, V> p in src) {
                    newMap[p.Key] = p.Value;
                }
            }
            return newMap;
        }

    }
}
