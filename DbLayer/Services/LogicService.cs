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
      DBStaticLogic obj2Update;
      obj2UpdateIn.CopyAllToAsJson(out obj2Update);

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

      StaticLogicDTO ret;
      result.CopyAllToAsJson(out ret);

      return ret;
    }

    public async Task<List<StaticLogicDTO>> GetByFigureAsync(string id)
    {
      List<StaticLogicDTO> listRet = new List<StaticLogicDTO>();

      //var filter = Builders<DBStaticLogic>.Filter.Eq("figs._id", new ObjectId(id));

      var filter = Builders<DBStaticLogic>.Filter
        .ElemMatch(x => x.figs, d => d.id == id);

      var result = await _coll
        .Find(filter)
        .ToListAsync();

      if (result == null)
      {
        return null;
      }

      foreach (var f in result)
      {
        StaticLogicDTO ret;
        f.CopyAllToAsJson(out ret);
        listRet.Add(ret);
      }      

      return listRet;
    }
  }
}
