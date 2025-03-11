using DbLayer.Models;
using Domain;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DbLayer.Services
{
  internal class IntegroService : IIntegroService, IIntegroServiceInternal
  {
    private IMongoCollection<DBIntegro> _coll;
    private readonly IMongoClient _mongoClient;
    private readonly IOptions<MapDatabaseSettings> _databaseSettings;

    private IMongoCollection<DBIntegro> Coll
    {
      get
      {
        CreateCollections();
        return _coll;
      }
    }

    public IntegroService(
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
        mongoDatabase.GetCollection<DBIntegro>
        (_databaseSettings.Value.IntegroCollectionName);

      CreateIndexes();
    }

    private void CreateIndexes()
    {
      {
        IndexKeysDefinition<DBIntegro> keys =
                new IndexKeysDefinitionBuilder<DBIntegro>()
                .Ascending(d => d.i_name)
                ;

        var indexModel = new CreateIndexModel<DBIntegro>(
          keys, new CreateIndexOptions()
          { Name = "i_name" }
        );

        Coll?.Indexes.CreateOneAsync(indexModel);
      }
      {
        IndexKeysDefinition<DBIntegro> keys =
                new IndexKeysDefinitionBuilder<DBIntegro>()
                .Ascending(d => d.i_type)
                ;

        var indexModel = new CreateIndexModel<DBIntegro>(
          keys, new CreateIndexOptions()
          { Name = "i_type" }
        );

        Coll?.Indexes.CreateOneAsync(indexModel);
      }
    }

    public async Task UpdateListAsync(List<IntegroDTO> obj2UpdateIn)
    {
      if (obj2UpdateIn.Where(i => string.IsNullOrEmpty(i.i_name) ||
      string.IsNullOrEmpty(i.id)).Any())
      {
        throw new Exception("id and i_name must not be empty for integro");
      }

      await MongoHelper.UpdateListAsync(
         Coll,
         obj2UpdateIn,
        PropertyCopy.ConvertDTO2DB<DBIntegro, IntegroDTO>);
    }

    public async Task RemoveAsync(List<string> ids)
    {
      await MongoHelper.RemoveAsync(Coll, ids);
    }

    public async Task<Dictionary<string, IntegroDTO>> GetListByIdsAsync(List<string> ids)
    {
      var result = await  MongoHelper.GetListByIdsAsync(
        Coll, 
        ids, 
        PropertyCopy.ConvertListDB2DTO<DBIntegro, IntegroDTO>);
      return result;
    }
    public async Task<Dictionary<string, IntegroDTO>> GetListByType(string i_name, string i_type)
    {
      var dbObjs = await Coll.Find(i => i.i_name == i_name && i.i_type == i_type).ToListAsync(); ;
      return PropertyCopy.ConvertListDB2DTO<DBIntegro, IntegroDTO>(dbObjs); 
    }
  }
}
