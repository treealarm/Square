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
using Domain.ServiceInterfaces;

namespace DbLayer.Services
{
  internal class IntegrationLeafsService: IIntegrationLeafsServiceInternal, IIntegrationLeafsService
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
    public async Task<Dictionary<string, IntegrationLeafsDTO>> GetByParentIdsAsync(
      List<string> parent_ids
    )
    {
      List<DBIntegrationLeafs> retVal;

        retVal = await Coll
                .Find(x => parent_ids.Contains(x.parent_id))
                .ToListAsync();

      return PropertyCopy.ConvertListDB2DTO<DBIntegrationLeafs, IntegrationLeafsDTO>(retVal);
    }

    async Task IIntegrationLeafsServiceInternal.UpdateListAsync(List<IntegrationLeafsDTO> obj2UpdateIn)
    {
      var dbUpdated = new Dictionary<IntegrationLeafsDTO, DBIntegrationLeafs>();
      var bulkWrites = new List<WriteModel<DBIntegrationLeafs>>();

      foreach (var item in obj2UpdateIn)
      {
        var updatedObj = PropertyCopy.ConvertDTO2DB<DBIntegrationLeafs, IntegrationLeafsDTO>(item);
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
    async Task IIntegrationLeafsServiceInternal.RemoveAsync(List<string> ids)
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
