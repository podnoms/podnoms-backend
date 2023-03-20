using System;
using System.Security.Cryptography;
using System.Text;

namespace PodNoms.Common.Utils.Crypt {
    public static class PBKDFGenerators {
        private const int LENGTH = 128;
        private const int WORK_FACTOR = 128;

        public static byte[] GenerateSalt(int length = LENGTH) {
            var bytes = new byte[length];

            using var rng = RandomNumberGenerator.Create("PDNM_SALTER");
            if (rng is not null) {
                rng.GetBytes(bytes);

                return bytes;
            }

            return Array.Empty<byte>();
        }

        public static byte[] GenerateHash(byte[] password, string salt, int length = LENGTH) {
            return GenerateHash(password, new UTF8Encoding().GetBytes(salt), length);
        }

        public static byte[] GenerateHash(byte[] password, byte[] salt, int length = LENGTH,
            int iterations = WORK_FACTOR) {
            using (var deriveBytes = new Rfc2898DeriveBytes(password, salt, iterations)) {
                return deriveBytes.GetBytes(length);
            }
        }
    }
}
