using System;
using System.Security.Cryptography;
using System.Text;

namespace PodNoms.Data.Utils;

public static class ApiKeyGenerator {
  private const int SaltSize = 32;
  private const int HashSize = 32;
  private const int IterationCount = 10000;

  internal static readonly char[] chars =
    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();

  public static string GetApiKey(int size) {
    var data = new byte[4 * size];
    using (var crypto = new RNGCryptoServiceProvider()) {
      crypto.GetBytes(data);
    }

    var result = new StringBuilder(size);
    for (var i = 0; i < size; i++) {
      var rnd = BitConverter.ToUInt32(data, i * 4);
      var idx = rnd % chars.Length;

      result.Append(chars[idx]);
    }

    return result.ToString();
  }

  public static string GeneratePasswordHash(string password, string saltData) {
    using var encoder = new Rfc2898DeriveBytes(
      password,
      new UTF8Encoding().GetBytes(saltData),
      IterationCount);

    var hashData = encoder.GetBytes(HashSize);
    return Convert.ToBase64String(hashData);
  }
}
