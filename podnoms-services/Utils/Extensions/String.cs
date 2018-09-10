using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace PodNoms.Api.Utils.Extensions {
    public static class StringExtensions {
        public static string StripNonXMLChars(this string str, float xmlVersion = 1.1f) {
            if (string.IsNullOrEmpty(str))
                return string.Empty;
            const string patternVersion1_0 = @"&#x((10?|[2-F])FFF[EF]|FDD[0-9A-F]|7F|8[0-46-9A-F]9[0-9A-F]);";
            const string patternVersion1_1 = @"&#x((10?|[2-F])FFF[EF]|FDD[0-9A-F]|[19][0-9A-F]|7F|8[0-46-9A-F]|0?[1-8BCEF]);";
            string Pattern = xmlVersion == 1.0f ? patternVersion1_0 : patternVersion1_1;
            string newString = string.Empty;
            Regex regex = new Regex(Pattern, RegexOptions.IgnoreCase);
            if (regex.IsMatch(str))
                newString = regex.Replace(str, "");
            else
                newString = str;

            //remove FUCKING EMOJI!!!!!!!!!
            string result = Regex.Replace(newString, @"\p{Cs}", "");
            return result;
        }

        public static string UrlParse(this string url, params string[] parts) {
            url = url.TrimEnd('/');
            foreach (var u in parts) {
                url = string.Format("{0}/{1}", url, u.TrimStart('/'));
            }
            return url;
        }
    }
}