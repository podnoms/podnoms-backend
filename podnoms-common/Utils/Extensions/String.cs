using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace PodNoms.Common.Utils.Extensions {
    public static class StringExtensions {
        public static string StripNonXmlChars(this string str, float xmlVersion = 1.1f) {
            if (string.IsNullOrEmpty(str))
                return string.Empty;
            const string patternVersion10 = @"&#x((10?|[2-F])FFF[EF]|FDD[0-9A-F]|7F|8[0-46-9A-F]9[0-9A-F]);";
            const string patternVersion11 = @"&#x((10?|[2-F])FFF[EF]|FDD[0-9A-F]|[19][0-9A-F]|7F|8[0-46-9A-F]|0?[1-8BCEF]);";
            var pattern = Math.Abs(xmlVersion - 1.0f) < 0.001 ? patternVersion10 : patternVersion11;
            var regex = new Regex(pattern, RegexOptions.IgnoreCase);
            var newString = regex.IsMatch(str) ? regex.Replace(str, "") : str;

            //remove FUCKING EMOJI!!!!!!!!!
            var result = Regex.Replace(newString, @"\p{Cs}", "");
            return result;
        }

        public static string ToTitleCase(this string phrase) {
            TextInfo textInfo = new CultureInfo("en-IE", false).TextInfo;
            return textInfo.ToTitleCase(phrase); 
        }
        
        public static string UrlParse(this string url, params string[] parts) {
            url = url.TrimEnd('/');
            return parts.Aggregate(url, (current, u) => string.Format("{0}/{1}", current, u.TrimStart('/')));
        }
        public static string FindStringFollowing(this string text, string searchTerm) {
            int pos = text.IndexOf(searchTerm, StringComparison.Ordinal);
            if (pos < 0) {
                return string.Empty;
            }

            string temp = text.Substring(pos + searchTerm.Length).Trim();
            string[] parts = temp.Split(' ');
            return parts[0];
        }
    }
}
