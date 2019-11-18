using System;
using System.Globalization;

namespace PodNoms.Common.Utils {
    public static class DateUtils {

        public static DateTime ParseBest(this string dateString) {
            var formats = new string[] {
                "dd/MM/yyyy",
                "dd-MM-yyyy",
                "yyyyMMdd",
                "yyyy-MM-dd",
                "dd/MM/yyyy",
                "dd/MM/yyyy",
                "d/MM/yyyy",
                "dd.MM.yyyy",
                "d/MM/yyyy",
                "d/MM/yyyy",
                "yyyy-M-d",
                "dd/MM/yyyy",
                "yyyy-MM-dd",
                "d.M.yyyy",
                "dd-MM-yyyy",
                "dd/MM/yyyy",
                "yyyy-MM-dd",
                "dd/MM/yyyy",
                "dd.MM.yyyy",
                "dd.MM.yyyy",
                "dd.MM.yyyy",
                "dd-MM-yyyy",
                "yyyy-M-d",
                "d/MM/yyyy",
                "dd/MM/yyyy",
                "dd/MM/yyyy",
                "d.M.yyyy",
                "dd.MM.yyyy",
                "dd-MM-yyyy",
                "MM/dd/yyyy",
                "dd/MM/yyyy",
                "dd/MM/yyyy",
                "dd/MM/yyyy",
                "d/MM/yyyy",
                "dd/MM/yyyy",
                "d.MM.yyyy",
                "d.M.yyyy",
                "dd/MM/yyyy",
                "dd/MM/yyyy",
                "d/M/yyyy",
                "d/MM/yyyy",
                "yyyy年M月d日",
                "MM-dd-yyyy",
                "dd.MM.yyyy.",
                "yyyy.MM.dd.",
                "dd/MM/yyyy",
                "३/६/१२",
                "d/M/yyyy",
                "dd/MM/yyyy",
                "dd/MM/yyyy",
                "dd/MM/yyyy",
                "d.M.yyyy",
                "dd/MM/yyyy",
                "dd/MM/yyyy",
                "dd/MM/yyyy",
                "yyyy/MM/dd",
                "H24.MM.dd",
                "yyyy. M. d",
                "dd/MM/yyyy",
                "dd/MM/yyyy",
                "dd/MM/yyyy",
                "yyyy.M.d",
                "dd/MM/yyyy",
                "dd.MM.yyyy",
                "yyyy.d.M",
                "dd/MM/yyyy",
                "d/MM/yyyy",
                "d.M.yyyy",
                "dd/MM/yyyy",
                "dd/MM/yyyy",
                "d.M.yyyy.",
                "dd/MM/yyyy",
                "MM-dd-yyyy",
                "d-M-yyyy",
                "dd.MM.yyyy",
                "dd.MM.yyyy",
                "d/MM/yyyy",
                "dd/MM/yyyy",
                "MM/dd/yyyy",
                "dd/MM/yyyy",
                "M/d/yyyy",
                "dd.MM.yyyy",
                "MM-dd-yyyy",
                "dd-MM-yyyy",
                "dd/MM/yyyy",
                "dd/MM/yyyy",
                "dd.MM.yyyy",
                "dd.MM.yyyy",
                "dd/MM/yyyy",
                "d.M.yyyy.",
                "dd/MM/yyyy",
                "dd/MM/yyyy",
                "M/d/yyyy",
                "MM-dd-yyyy",
                "d.M.yyyy.",
                "d.M.yyyy",
                "d.M.yyyy",
                "yyyy-MM-dd",
                "dd/MM/yyyy",
                "d/M/2555",
                "๓/๖/๒๕๕๕",
                "dd/MM/yyyy",
                "dd.MM.yyyy",
                "yyyy/M/d",
                "dd.MM.yyyy",
                "dd/MM/yyyy",
                "M/d/yyyy",
                "M/d/yyyy",
                "dd/MM/yyyy",
                "dd/MM/yyyy",
                "dd/MM/yyyy",
                "yyyy/MM/dd"
            };
            try {
                if (!DateTime.TryParse(dateString, out var result)) {
                    result = DateTime.ParseExact(
                        dateString,
                        formats,
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeLocal);
                }
                return result;
            } catch (Exception) {
                return System.DateTime.Today;
            }
        }
        public static DateTime ConvertFromUnixTimestamp(double timestamp) {
            var origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return origin.AddSeconds(timestamp);
        }

        public static double ConvertToUnixTimestamp(DateTime date) {
            var origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            var diff = date.ToUniversalTime() - origin;
            return Math.Floor(diff.TotalSeconds);
        }
    }
}
