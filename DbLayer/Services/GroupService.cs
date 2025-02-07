using DbLayer.Models;
using Domain;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DbLayer.Services
{
  internal class GroupsService : IGroupsService, IIGroupsServiceInternal
  {
    private IMongoCollection<DBGroup> _coll;
    private readonly IMongoClient _mongoClient;
    private readonly IOptions<MapDatabaseSettings> _databaseSettings;

    private IMongoCollection<DBGroup> Coll
    {
      get
      {
        CreateCollections();
        return _coll;
      }
    }

    public GroupsService(
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
        mongoDatabase.GetCollection<DBGroup>
        (_databaseSettings.Value.GroupsCollectionName);

      CreateIndexes();
    }

    private void CreateIndexes()
    {
      {
        IndexKeysDefinition<DBGroup> keys =
                new IndexKeysDefinitionBuilder<DBGroup>()
                .Ascending(d => d.name)
                ;

        var indexModel = new CreateIndexModel<DBGroup>(
          keys, new CreateIndexOptions()
          { Name = "name" }
        );

        Coll?.Indexes.CreateOneAsync(indexModel);
      }
      {
        IndexKeysDefinition<DBGroup> keys =
                new IndexKeysDefinitionBuilder<DBGroup>()
                .Ascending(d => d.objid)
                ;

        var indexModel = new CreateIndexModel<DBGroup>(
          keys, new CreateIndexOptions()
          { Name = "objid" }
        );

        Coll?.Indexes.CreateOneAsync(indexModel);
      }
    }

    public static Dictionary<string, GroupDTO> ConvertListDB2DTO(List<DBGroup> dbObjs)
    {
      var result = new Dictionary<string, GroupDTO>();

      foreach (var dbItem in dbObjs)
      {
        result.Add(dbItem.id, ConvertDB2DTO(dbItem)!);
      }

      return result;
    }

    public static GroupDTO ConvertDB2DTO(DBGroup dbo)
    {
      if (dbo == null)
      {
        return null;
      }

      var dto = new GroupDTO()
      {
        id = dbo.id,
        name = dbo.name,
        objid = dbo.objid
      };

      return dto;
    }
    public static DBGroup ConvertDTO2DB(GroupDTO dto)
    {
      if (dto == null)
      {
        return default;
      }

      var dbo = new DBGroup()
      {
        id = string.IsNullOrEmpty(dto.id) ? null : dto.id,
        name = dto.name,
        objid = dto.objid
      };

      return dbo;
    }

    public async Task<Dictionary<string, GroupDTO>> GetListByIdsAsync(List<string> ids)
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

    public async Task UpdateListAsync(List<GroupDTO> valuesToUpdate)
    {
      await MongoHelper.UpdateListAsync(
        Coll,
        valuesToUpdate,
        ConvertDTO2DB);
    }

    public async Task<Dictionary<string, GroupDTO>> GetListByNamesAsync(List<string> names)
    {
      var ret = await Coll.Find(g => names.Contains(g.name)).ToListAsync();
      return ConvertListDB2DTO(ret);
    }

    public async Task RemoveByNameAsync(List<string> names)
    {
      await Coll.DeleteManyAsync(g=> names.Contains(g.name));
    }
  }
}
