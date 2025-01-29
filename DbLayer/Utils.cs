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
        // Получаем сериализатор для типа T
        var serializerRegistry = BsonSerializer.SerializerRegistry;
        var documentSerializer = serializerRegistry.GetSerializer<T>();

        var renderArgs = new RenderArgs<T>
        {
          DocumentSerializer = documentSerializer,
          // Можно добавить другие параметры, если это нужно, например, настройки сериализации.
        };
        // Рендерим фильтр
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
