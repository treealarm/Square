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
    static public string GenerateId24()
    {
      return MongoDB.Bson.ObjectId.GenerateNewId().ToString();
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

