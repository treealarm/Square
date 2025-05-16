using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System.Diagnostics;
using System;
using MongoDB.Bson;
using System.Security.Cryptography;

namespace DbLayer
{
  public class Utils
  {
    static public string GenerateId24()
    {
      return MongoDB.Bson.ObjectId.GenerateNewId().ToString();
    }
    //public static string GenerateObjectId()
    //{
    //  byte[] bytes = new byte[12];
    //  RandomNumberGenerator.Fill(bytes); // Заполняем случайными байтами
    //  return BitConverter.ToString(bytes).Replace("-", "").ToLower(); // Преобразуем в строку hex
    //}
    public static Guid ConvertObjectIdToGuid(string objectIdString)
    {
      if (Guid.TryParse(objectIdString, out Guid id)) 
      { 
        return id; 
      }

      if (objectIdString?.Length != 24)
        throw new ArgumentException("Invalid ObjectId length", nameof(objectIdString));

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

    public static string Log<BsonDocument>(FilterDefinition<BsonDocument> filter)
    {
      string s = string.Empty;
      try
      {
        var serializerRegistry = BsonSerializer.SerializerRegistry;
        var documentSerializer = serializerRegistry.GetSerializer<BsonDocument>();

        var renderArgs = new RenderArgs<BsonDocument>
        {
          DocumentSerializer = documentSerializer
        };

        var rendered = filter.Render(renderArgs);

        // Выводим фильтр в формате JSON с отступами для читаемости
        s = rendered.ToJson(new JsonWriterSettings { Indent = true });
        Debug.WriteLine(s);
        Debug.WriteLine("");
      }
      catch (Exception ex)
      {
        Debug.WriteLine($"Ошибка при логировании фильтра: {ex.Message}");
      }
      return s;
    }

  }

}

