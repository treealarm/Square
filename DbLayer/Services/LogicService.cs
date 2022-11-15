using DbLayer.Models;
using Domain;
using Domain.ObjectInterfaces;
using Domain.ServiceInterfaces;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbLayer.Services
{
  public class LogicService: ILogicService
  {
    private readonly IMongoCollection<DBStaticLogic> _coll;
    private readonly MongoClient _mongoClient;
    public LogicService(IOptions<MapDatabaseSettings> geoStoreDatabaseSettings)
    {
      _mongoClient = new MongoClient(
        geoStoreDatabaseSettings.Value.ConnectionString);

      var mongoDatabase = _mongoClient.GetDatabase(
          geoStoreDatabaseSettings.Value.DatabaseName);

      _coll =
        mongoDatabase.GetCollection<DBStaticLogic>
        (geoStoreDatabaseSettings.Value.LogicCollectionName);

      CreateIndexes();
    }

    private void CreateIndexes()
    {
      {
        IndexKeysDefinition<DBStaticLogic> keys =
                new IndexKeysDefinitionBuilder<DBStaticLogic>()
                .Ascending(d => d.figs)
                ;

        var indexModel = new CreateIndexModel<DBStaticLogic>(
          keys, new CreateIndexOptions()
          { Name = "figs" }
        );

        _coll.Indexes.CreateOneAsync(indexModel);
      }
    }

    public async Task UpdateAsync(StaticLogicDTO obj2UpdateIn)
    {
      DBStaticLogic obj2Update = new DBStaticLogic();
      obj2UpdateIn.CopyAllTo(obj2Update);

      if (string.IsNullOrEmpty(obj2Update.id))
      {
        await _coll.InsertOneAsync(obj2Update);
        obj2UpdateIn.id = obj2Update.id;
      }
      else
      {
        var options = new ReplaceOptions() { IsUpsert = true };
        await _coll.ReplaceOneAsync(x => x.id == obj2Update.id, obj2Update, options);
      }        
    }

    public async Task DeleteAsync(string id)
    {
      await _coll.DeleteOneAsync(x => x.id == id);
    }

    public async Task<StaticLogicDTO> GetAsync(string id)
    {
      var result = await _coll.Find(x => x.id == id).FirstOrDefaultAsync();

      if (result == null)
      {
        return null;
      }

      StaticLogicDTO ret = new StaticLogicDTO();
      result.CopyAllTo(ret);

      return ret;
    }
  }
}
