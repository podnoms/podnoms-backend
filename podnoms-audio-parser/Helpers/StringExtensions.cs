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
    }
}
