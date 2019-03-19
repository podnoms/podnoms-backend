using System.Security.Cryptography;

namespace PodNoms.Common.Utils.Crypt {
    public static class PBKDFGenerators {
        private const int LENGTH = 128;
        private const int WORK_FACTOR = 128;

        public static byte[] GenerateSalt(int length = LENGTH) {
            var bytes = new byte[length];

            using (var rng = new RNGCryptoServiceProvider()) {
                rng.GetBytes(bytes);
            }

            return bytes;
        }

        public static byte[] GenerateHash(byte[] password, byte[] salt, int length = LENGTH,
            int iterations = WORK_FACTOR) {
            using (var deriveBytes = new Rfc2898DeriveBytes(password, salt, iterations)) {
                return deriveBytes.GetBytes(length);
            }
        }
    }
}