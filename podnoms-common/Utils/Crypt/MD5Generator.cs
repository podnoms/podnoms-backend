using System.Security.Cryptography;
using System.Text;

namespace PodNoms.Common.Utils.Crypt {
    public class MD5Generator {
        public static string CalculateMD5Hash(string input) {
            // step 1, calculate MD5 hash from input
            var md5 = MD5.Create();
            var inputBytes = Encoding.ASCII.GetBytes(input);
            var hash = md5.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string
            var sb = new StringBuilder();
            foreach (var t in hash) {
                sb.Append(t.ToString("X2"));
            }
            return sb.ToString();
        }
    }
}