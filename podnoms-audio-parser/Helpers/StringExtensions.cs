using System.Text.Json;

namespace PodNoms.AudioParsing.Helpers {
    public static class StringExtensions {
        public static string ReplaceEnd(this string str, string toReplace, string replaceWith) {
            return str.EndsWith(toReplace)
                ? $"{str[..^toReplace.Length]}{replaceWith}"
                : str;
        }

        public static string RemoveFromEnd(this string str, string toRemove) {
            return str.EndsWith(toRemove) ? str[..^toRemove.Length] : str;
        }

        public static bool IsJson(this string source) {
            if (source == null)
                return false;
            try {
                JsonDocument.Parse(source);
                return true;
            } catch (JsonException) {
                return false;
            }
        }
    }
}
