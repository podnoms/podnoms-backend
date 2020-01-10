using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace PodNoms.Data.Extensions {
    public static class StringExtensions {
        public static string Slugify(this string phrase, IEnumerable<string> source) {
            var str = phrase.RemoveAccent().ToLower().RemoveInvalidUrlChars().RemoveNonAlphaChars();
            // invalid chars           
            str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
            // convert multiple spaces into one space   
            str = Regex.Replace(str, @"\s+", " ").Trim();
            // cut and trim 
            str = str.Substring(0, str.Length <= 45 ? str.Length : 45).Trim();
            str = Regex.Replace(str, @"\s", "-"); // hyphens   
            str = str.RemoveAccent().ToLower();

            str = str.Replace(" ", "");
            var count = 1;
            var origStr = str;
            while (source != null && source.Count() != 0 &&
                !string.IsNullOrEmpty(source.Where(e => e == str).Select(e => e).DefaultIfEmpty("").FirstOrDefault())) {
                str = $"{origStr}-{count++}";
            }
            return str;
        }

        public static string RemoveNonAlphaChars(this string str) {
            var rgx = new Regex("[^a-zA-Z0-9 -]");
            return rgx.Replace(str, "");
        }
        public static string RemoveInvalidUrlChars(this string str) {
            var regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            var r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
            return r.Replace(str, "");
        }
        public static string RemoveAccent(this string txt) {
            var bytes = Encoding.GetEncoding("Cyrillic").GetBytes(txt);
            return Encoding.ASCII.GetString(bytes);
        }
        public static string Truncate(this string value, int maxChars) {
            return value.Length <= maxChars ? value : value.Substring(0, maxChars) + "...";
        }
    }
}
