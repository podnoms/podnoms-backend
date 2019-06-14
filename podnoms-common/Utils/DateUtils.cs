using System;

namespace PodNoms.Common.Utils {
    public class DateUtils {
        public static DateTime ConvertFromUnixTimestamp (double timestamp) {
            var origin = new DateTime (1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return origin.AddSeconds (timestamp);
        }

        public static double ConvertToUnixTimestamp (DateTime date) {
            var origin = new DateTime (1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            var diff = date.ToUniversalTime () - origin;
            return Math.Floor (diff.TotalSeconds);
        }
    }
}
