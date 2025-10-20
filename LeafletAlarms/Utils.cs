using System.Security.Cryptography;
using System.Text;

namespace LeafletAlarms
{
  public static class Utils
  {
    static public string GenerateObjectId(string input, string version)
    {
      input ??= string.Empty;
      version ??= string.Empty;

      // Объединяем строки
      string combined = input + version;

      // Получаем MD5-хэш (16 байт — идеально для Guid)
      using var md5 = MD5.Create();
      byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(combined));

      // Создаем Guid из хэша
      return new Guid(hash).ToString();
    }
  }
}
