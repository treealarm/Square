using System;
using DbLayer.Models.Integration;
using Domain;
using Domain.Diagram;
using Domain.Integration;
using Domain.OptionsModels;
using Domain.ServiceInterfaces;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DbLayer.Services
{
  internal class IntegrationService: IIntegrationServiceInternal,  IIntegrationService
  {
    private readonly IMongoCollection<DBIntegration> _coll;
    private readonly IMongoClient _mongoClient;
    public IntegrationService(
      IOptions<MapDatabaseSettings> geoStoreDatabaseSettings,
      IMongoClient mongoClient)
    {
      _mongoClient = mongoClient;

      var mongoDatabase = _mongoClient.GetDatabase(
          geoStoreDatabaseSettings.Value.DatabaseName);

      _coll =
        mongoDatabase.GetCollection<DBIntegration>
        (geoStoreDatabaseSettings.Value.IntegrationCollectionName);

      CreateIndexes();
    }

    private void CreateIndexes()
    {
      {
        var keys =
                new IndexKeysDefinitionBuilder<DBIntegration>()
                .Ascending(d => d.name)
                .Ascending(d => d.parent_id)
                ;

        var indexModel = new CreateIndexModel<DBIntegration>(
          keys, new CreateIndexOptions()
          { Name = "base" }
        );

        _coll.Indexes.CreateOneAsync(indexModel);
      }
    }
    async Task<Dictionary<string, IntegrationDTO>> IIntegrationService.GetByParentIdsAsync(
      List<string> parent_ids,
      string start_id,
      string end_id,
      int count
    )
    {
      List<DBIntegration> retVal;

      if (start_id != null)
      {
        var filter = Builders<DBIntegration>.Filter.Gte("_id", new ObjectId(start_id))
          & Builders<DBIntegration>.Filter.In("parent_id", parent_ids);

        retVal = await _coll
          .Find(filter)
          .Limit(count)
          .ToListAsync();
      }
      else if (end_id != null)
      {
        var filter = Builders<DBIntegration>.Filter.Lte("_id", new ObjectId(end_id))
          & Builders<DBIntegration>.Filter.In("parent_id", parent_ids);

        retVal = await _coll
          .Find(filter)
          .SortByDescending(x => x.id)
          .Limit(count)
          .ToListAsync()
          ;

        retVal.Sort((x, y) => new ObjectId(x.id).CompareTo(new ObjectId(y.id)));
      }
      else
      {
        retVal = await _coll
                .Find(x => parent_ids.Contains(x.parent_id))
                .Limit(count)
                .ToListAsync();
      }

      return PropertyCopy.ConvertListDB2DTO<DBIntegration, IntegrationDTO>(retVal);
    }

    public async Task<Dictionary<string, bool>> GetHasChildrenAsync(List<string> parent_ids)
    {
      Dictionary<string, bool> retVal = new Dictionary<string, bool>();

      try
      {
        // Фильтр по parent_ids
        var filter = Builders<DBIntegration>.Filter.In(x => x.parent_id, parent_ids);

        // Агрегирование для проверки наличия хотя бы одного дочернего элемента для каждого parent_id
        var aggregation = await _coll.Aggregate()
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

    async Task IIntegrationServiceInternal.UpdateListAsync(List<IntegrationDTO> obj2UpdateIn)
    {
      var dbUpdated = new Dictionary<IntegrationDTO, DBIntegration>();
      var bulkWrites = new List<WriteModel<DBIntegration>>();

      foreach (var item in obj2UpdateIn)
      {
        var updatedObj = PropertyCopy.ConvertDTO2DB<DBIntegration, IntegrationDTO>(item);
        dbUpdated.Add(item, updatedObj);
        var filter = Builders<DBIntegration>.Filter.Eq(x => x.id, updatedObj.id);

        if (string.IsNullOrEmpty(updatedObj.parent_id))
        {
          updatedObj.parent_id = null;
        }

        if (string.IsNullOrEmpty(updatedObj.id))
        {
          updatedObj.id = ObjectId.GenerateNewId().ToString();
          var request = new InsertOneModel<DBIntegration>(updatedObj);
          bulkWrites.Add(request);
        }
        else
        {
          var request = new ReplaceOneModel<DBIntegration>(filter, updatedObj);
          request.IsUpsert = true;
          bulkWrites.Add(request);
        }
      }

      var writeResult = await _coll.BulkWriteAsync(bulkWrites);

      foreach (var pair in dbUpdated)
      {
        pair.Key.id = pair.Value.id;
      }
    }
    async Task IIntegrationServiceInternal.RemoveAsync(List<string> ids)
    {
      await _coll.DeleteManyAsync(
          x => ids.Contains(x.id));
    }
  }
}
