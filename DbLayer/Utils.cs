using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System.Diagnostics;
using System;
using MongoDB.Bson;

namespace DbLayer
{
  public class Utils
  {
    static public string GenerateBsonId()
    {
      return MongoDB.Bson.ObjectId.GenerateNewId().ToString();
    }

    public static string Log<T>(FilterDefinition<T> filter)
    {
      string s = string.Empty;
      try
      {
        // Преобразуем фильтр в BsonDocument, что позволяет обрабатывать различные типы данных без специфичных сериализаторов
        var rendered = filter.Render(new RenderArgs<T>());

        // Преобразуем в JSON строку с отступами для читаемости
        s = rendered.ToJson(new JsonWriterSettings { Indent = true });

        // Выводим результат
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
