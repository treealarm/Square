using System.Diagnostics;
using System;

namespace Domain
{
  public class Utils
  {
    public static Guid? ConvertObjectIdToGuid(string objectIdString)
    {
      if (Guid.TryParse(objectIdString, out Guid id)) 
      { 
        return id; 
      }

      if (objectIdString?.Length != 24)
        return null;

      // Преобразуем строку ObjectId в массив байтов (каждый символ - это 4 бита, 24 символа = 12 байт)
      byte[] objectIdBytes = new byte[12];
      for (int i = 0; i < 12; i++)
      {
        objectIdBytes[i] = Convert.ToByte(objectIdString.Substring(i * 2, 2), 16);
      }

      // Преобразуем первые 16 байт в UUID (если необходимо, добавьте дополнительные байты)
      byte[] uuidBytes = new byte[16];
      Array.Copy(objectIdBytes, uuidBytes, 12); // Копируем первые 12 байт

      return new Guid(uuidBytes);
    }
    public static string ConvertGuidToObjectId(Guid guid)
    {
      byte[] guidBytes = guid.ToByteArray();
      byte[] objectIdBytes = new byte[12];
      Array.Copy(guidBytes, objectIdBytes, 12); // Копируем первые 12 байт

      // Преобразуем байты обратно в строку
      string objectIdString = BitConverter.ToString(objectIdBytes).Replace("-", "").ToLower();
      return objectIdString;
    }
  }
}

