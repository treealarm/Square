using DbLayer.Models;
using DbLayer.Models.Values;
using Domain;
using Domain.OptionsModels;
using Domain.ServiceInterfaces;
using Domain.Values;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DbLayer.Services
{
  internal class ValuesService : IValuesService, IValuesServiceInternal
  {
    private IMongoCollection<DBValue> _coll;
    private readonly IMongoClient _mongoClient;
    private readonly IOptions<MapDatabaseSettings> _databaseSettings;

    private IMongoCollection<DBValue> Coll
    {
      get
      {
        CreateCollections();
        return _coll;
      }
    }

    public ValuesService(
      IOptions<MapDatabaseSettings> databaseSettings,
      IMongoClient mongoClient)
    {
      _mongoClient = mongoClient;
      _databaseSettings = databaseSettings;
      CreateCollections();

    }

    private void CreateCollections()
    {
      var mongoDatabase = _mongoClient.GetDatabase(
          _databaseSettings.Value.DatabaseName);

      if (_coll != null)
      {
        return;
      }
      _coll =
        mongoDatabase.GetCollection<DBValue>
        (_databaseSettings.Value.ValuesCollectionName);

      CreateIndexes();
    }

    private void CreateIndexes()
    {
      {
        IndexKeysDefinition<DBValue> keys =
                new IndexKeysDefinitionBuilder<DBValue>()
                .Ascending(d => d.owner_id)
                ;

        var indexModel = new CreateIndexModel<DBValue>(
          keys, new CreateIndexOptions()
          { Name = "owner" }
        );

        Coll?.Indexes.CreateOneAsync(indexModel);
      }
    }

    public static Dictionary<string, ValueDTO> ConvertListDB2DTO(List<DBValue> dbObjs)
    {
      var result = new Dictionary<string, ValueDTO>();

      foreach (var dbItem in dbObjs)
      {
        result.Add(dbItem.id, ConvertDB2DTO(dbItem)!);
      }

      return result;
    }

    public async Task<Dictionary<string, ValueDTO>> GetListByIdsAsync(List<string> ids)
    {
      var result = await MongoHelper.GetListByIdsAsync(
        Coll,
        ids,
        ConvertListDB2DTO);

      return result;
    }

    public async Task RemoveAsync(List<string> ids)
    {
      await MongoHelper.RemoveAsync(Coll, ids);
    }

    private static BsonValue ConvertToBsonValue(object value)
    {
      if (value == null) return BsonNull.Value;

      if (value is System.Text.Json.JsonElement jsonElement)
      {
        return BsonTypeMapper.MapToBsonValue(ConvertJsonElementToObject(jsonElement));
      }

      return BsonTypeMapper.MapToBsonValue(value);
      //// Если value — JsonElement, преобразуем
      //if (value is System.Text.Json.JsonElement jsonElement)
      //{
      //  value = ConvertJsonElementToObject(jsonElement);
      //}

      //return BsonValue.Create(value);
    }

    private static object ConvertJsonElementToObject(System.Text.Json.JsonElement element)
    {
      switch (element.ValueKind)
      {
        case System.Text.Json.JsonValueKind.Object:
          return element.EnumerateObject()
              .ToDictionary(prop => prop.Name, prop => ConvertJsonElementToObject(prop.Value));
        case System.Text.Json.JsonValueKind.Array:
          return element.EnumerateArray()
              .Select(ConvertJsonElementToObject)
              .ToList();
        case System.Text.Json.JsonValueKind.String:
          return element.GetString();
        case System.Text.Json.JsonValueKind.Number:
          if (element.TryGetInt32(out var intValue))
            return intValue;
          return element.GetDouble();
        case System.Text.Json.JsonValueKind.True:
        case System.Text.Json.JsonValueKind.False:
          return element.GetBoolean();
        case System.Text.Json.JsonValueKind.Null:
          return null;
        default:
          throw new NotSupportedException($"JsonElement типа {element.ValueKind} не поддерживается.");
      }
    }

    public static object ConvertFromBsonValue(BsonValue bsonValue)
    {
      if (bsonValue == null || bsonValue.IsBsonNull)
      {
        return null;
      }
      return BsonTypeMapper.MapToDotNetValue(bsonValue);
      //return bsonValue switch
      //{
      //  BsonString bsonString => bsonString.AsString,
      //  BsonInt32 bsonInt32 => bsonInt32.AsInt32,
      //  BsonInt64 bsonInt64 => bsonInt64.AsInt64,
      //  BsonDouble bsonDouble => bsonDouble.AsDouble,
      //  BsonBoolean bsonBoolean => bsonBoolean.AsBoolean,
      //  BsonDateTime bsonDateTime => bsonDateTime.ToUniversalTime(),
      //  BsonArray bsonArray => bsonArray.Select(ConvertFromBsonValue).ToList(),
      //  BsonDocument bsonDocument => bsonDocument.ToDictionary(),
      //  _ => bsonValue.RawValue
      //};
    }

    public static DBValue ConvertDTO2DB(ValueDTO dto)
    {
      if (dto == null)
      {
        return default;
      }

      var dbo = new DBValue()
      {
        id = string.IsNullOrEmpty(dto.id) ? null : dto.id,
        name = dto.name,
        owner_id = dto.owner_id,
        value = ConvertToBsonValue(dto.value)
      };

      return dbo;
    }

    public static ValueDTO ConvertDB2DTO(DBValue dbo)
    {
      if (dbo == null)
      {
        return null;
      }

      var dto = new ValueDTO()
      {
        id = dbo.id,
        name = dbo.name,
        owner_id = dbo.owner_id,
        value = ConvertFromBsonValue(dbo.value)
      };

      return dto;
    }


    public async Task UpdateListAsync(List<ValueDTO> valuesToUpdate)
    {
      await MongoHelper.UpdateListAsync(
        Coll,
        valuesToUpdate,
       ConvertDTO2DB);
    }

    public async Task<Dictionary<string, ValueDTO>> GetListByOwnersAsync(List<string> owners)
    {
      var retVal = await Coll.Find(i => owners.Contains(i.owner_id)).ToListAsync();
      return ConvertListDB2DTO(retVal);
    }


    public async Task<Dictionary<string, ValueDTO>> UpdateValuesFilteredByNameAsync(List<ValueDTO> obj2UpdateIn)
    {
      var bulkWrites = new List<WriteModel<DBValue>>();

      foreach (var item in obj2UpdateIn)
      {
        var updatedObj = ConvertDTO2DB(item);
        // Создание фильтра
        var filter = !string.IsNullOrEmpty(updatedObj.id)
            ? Builders<DBValue>.Filter.Eq(x => x.id, updatedObj.id)
            : Builders<DBValue>.Filter.Where(x => x.owner_id == updatedObj.owner_id && x.name == updatedObj.name);

        // Удаляем id перед upsert, если он пустой
        if (string.IsNullOrEmpty(updatedObj.id))
        {
          updatedObj.id = null;
        }

        // Создаём запрос
        var request = new ReplaceOneModel<DBValue>(filter, updatedObj)
        {
          IsUpsert = true
        };
        bulkWrites.Add(request);
      }

      // Выполняем BulkWrite
      var retDocuments = await Coll.BulkWriteAsync(bulkWrites);

      var filter_check = Builders<DBValue>.Filter.Or(
        obj2UpdateIn.Select(item =>
            Builders<DBValue>.Filter.And(
                Builders<DBValue>.Filter.Eq(x => x.owner_id, item.owner_id),
                Builders<DBValue>.Filter.Eq(x => x.name, item.name)
            )
        ).ToArray()
      );
      var documents = await Coll.Find(filter_check).ToListAsync();
      var dbUpdated = new Dictionary<string, ValueDTO>();

      foreach (var doc in documents)
      {
          dbUpdated[doc.id] = ConvertDB2DTO(doc);          
      }

      return dbUpdated;
    }
  }
}
