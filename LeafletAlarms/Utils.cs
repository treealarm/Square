using System.Security.Cryptography;
using System.Text;

namespace LeafletAlarms
{
  public static class Utils
  {
    static public string GenerateObjectId(string input, string version)
    {
      if (string.IsNullOrEmpty(input))
      {
        input = string.Empty;
      }
      if (string.IsNullOrEmpty(version))
      {
        version = string.Empty;
      }
      string GetHash(string data, int length)
      {
        using (var md5 = MD5.Create())
        {
          var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(data));
          // Получаем нужное количество символов из хэша
          return BitConverter.ToString(hash, 0, length).Replace("-", "").ToLowerInvariant();
        }
      }

      // Получаем хэш для input и version
      string inputHash = GetHash(input + version, 12);

      return inputHash;
    }
  }
}
