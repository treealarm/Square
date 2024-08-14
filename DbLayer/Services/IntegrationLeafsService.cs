using System;
using Domain;
using Domain.Integration;
using Domain.OptionsModels;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DbLayer.Models;

namespace DbLayer.Services
{
  internal class IntegrationLeafsService
  {
    private IMongoCollection<DBIntegrationLeafs> _coll;
    private readonly IOptions<MapDatabaseSettings> _geoStoreDatabaseSettings;
    private IMongoCollection<DBIntegrationLeafs> Coll
    {
      get
      {
        if (_coll == null)
        {
          var mongoDatabase = _mongoClient.GetDatabase(
            _geoStoreDatabaseSettings.Value.DatabaseName);
          _coll =
          mongoDatabase.GetCollection<DBIntegrationLeafs>
          (_geoStoreDatabaseSettings.Value.IntegrationLeafsCollectionName);

          CreateIndexes();
        }
        return _coll;
      }
    }
    private readonly IMongoClient _mongoClient;
    public IntegrationLeafsService(
      IOptions<MapDatabaseSettings> geoStoreDatabaseSettings,
      IMongoClient mongoClient)
    {
      _mongoClient = mongoClient;
      _geoStoreDatabaseSettings = geoStoreDatabaseSettings;
    }

    private void CreateIndexes()
    {
      {
        var keys =
                new IndexKeysDefinitionBuilder<DBIntegrationLeafs>()
                .Ascending(d => d.parent_id)
                ;

        var indexModel = new CreateIndexModel<DBIntegrationLeafs>(
          keys, new CreateIndexOptions()
          { Name = "base" }
        );

        Coll.Indexes.CreateOneAsync(indexModel);
      }
    }
    async Task<Dictionary<string, IntegrationDTO>> GetByParentIdsAsync(
      List<string> parent_ids,
      string start_id,
      string end_id,
      int count
    )
    {
      List<DBIntegrationLeafs> retVal;

      if (start_id != null)
      {
        var filter = Builders<DBIntegrationLeafs>.Filter.Gte("_id", new ObjectId(start_id))
          & Builders<DBIntegrationLeafs>.Filter.In("parent_id", parent_ids);

        retVal = await Coll
          .Find(filter)
          .Limit(count)
          .ToListAsync();
      }
      else if (end_id != null)
      {
        var filter = Builders<DBIntegrationLeafs>.Filter.Lte("_id", new ObjectId(end_id))
          & Builders<DBIntegrationLeafs>.Filter.In("parent_id", parent_ids);

        retVal = await Coll
          .Find(filter)
          .SortByDescending(x => x.id)
          .Limit(count)
          .ToListAsync()
          ;

        retVal.Sort((x, y) => new ObjectId(x.id).CompareTo(new ObjectId(y.id)));
      }
      else
      {
        retVal = await Coll
                .Find(x => parent_ids.Contains(x.parent_id))
                .Limit(count)
                .ToListAsync();
      }

      return PropertyCopy.ConvertListDB2DTO<DBIntegrationLeafs, IntegrationDTO>(retVal);
    }

    public async Task<Dictionary<string, bool>> GetHasChildrenAsync(List<string> parent_ids)
    {
      Dictionary<string, bool> retVal = new Dictionary<string, bool>();

      try
      {
        // Фильтр по parent_ids
        var filter = Builders<DBIntegrationLeafs>.Filter.In(x => x.parent_id, parent_ids);

        // Агрегирование для проверки наличия хотя бы одного дочернего элемента для каждого parent_id
        var aggregation = await Coll.Aggregate()
            .Match(filter) // Фильтрация документов по parent_ids
            .Group(new BsonDocument
            {
            { "_id", "$parent_id" }, // Группировка по parent_id
            { "count", new BsonDocument("$sum", 1) } // Подсчет количества документов в группе
            })
            .ToListAsync();

        // Заполняем retVal на основе результатов агрегации
        foreach (var parent_id in parent_ids)
        {
          // Проверяем, есть ли запись для текущего parent_id в результатах агрегации
          var hasChildren = aggregation.Any(x => x["_id"].ToString() == parent_id);
          retVal[parent_id] = hasChildren; // Записываем результат в словарь
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.ToString());
      }


      return retVal;
    }

    async Task UpdateListAsync(List<IntegrationDTO> obj2UpdateIn)
    {
      var dbUpdated = new Dictionary<IntegrationDTO, DBIntegrationLeafs>();
      var bulkWrites = new List<WriteModel<DBIntegrationLeafs>>();

      foreach (var item in obj2UpdateIn)
      {
        var updatedObj = PropertyCopy.ConvertDTO2DB<DBIntegrationLeafs, IntegrationDTO>(item);
        dbUpdated.Add(item, updatedObj);
        var filter = Builders<DBIntegrationLeafs>.Filter.Eq(x => x.id, updatedObj.id);

        if (string.IsNullOrEmpty(updatedObj.parent_id))
        {
          updatedObj.parent_id = null;
        }

        if (string.IsNullOrEmpty(updatedObj.id))
        {
          updatedObj.id = ObjectId.GenerateNewId().ToString();
          var request = new InsertOneModel<DBIntegrationLeafs>(updatedObj);
          bulkWrites.Add(request);
        }
        else
        {
          var request = new ReplaceOneModel<DBIntegrationLeafs>(filter, updatedObj);
          request.IsUpsert = true;
          bulkWrites.Add(request);
        }
      }

      var writeResult = await Coll.BulkWriteAsync(bulkWrites);

      foreach (var pair in dbUpdated)
      {
        pair.Key.id = pair.Value.id;
      }
    }
    async Task RemoveAsync(List<string> ids)
    {
      var allIdsToDelete = new HashSet<string>(ids);

      foreach (var id in ids)
      {
        await CollectChildIds(id, allIdsToDelete);
      }
      await Coll.DeleteManyAsync(x => allIdsToDelete.Contains(x.id));
    }

    private async Task CollectChildIds(string parentId, HashSet<string> allIdsToDelete)
    {
      var children = await Coll.Find(x => x.parent_id == parentId).ToListAsync();

      foreach (var child in children)
      {
        if (allIdsToDelete.Add(child.id))
        {
          await CollectChildIds(child.id, allIdsToDelete);
        }
      }
    }
  }
}
