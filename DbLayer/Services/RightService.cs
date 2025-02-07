using DbLayer.Models;
using Domain;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DbLayer.Services
{
  internal class RightService: IRightService
  {
    private readonly IMongoCollection<DBObjectRights> _coll;
    private readonly IMongoClient _mongoClient;
    public  RightService(IOptions<MapDatabaseSettings> geoStoreDatabaseSettings, IMongoClient mongoClient) 
    {
      _mongoClient = mongoClient;

      var mongoDatabase = _mongoClient.GetDatabase(
          geoStoreDatabaseSettings.Value.DatabaseName);

      _coll =
        mongoDatabase.GetCollection<DBObjectRights>
        (geoStoreDatabaseSettings.Value.RightsCollectionName);

      CreateIndexes();
    }

    private void CreateIndexes()
    {
      {
        var keys = Builders<DBObjectRights>.IndexKeys.Combine(
          Builders<DBObjectRights>.IndexKeys
          .Ascending($"{nameof(DBObjectRights.rights)}.{nameof(DBObjectRightValue.role)}"),
          Builders<DBObjectRights>.IndexKeys
          .Ascending($"{nameof(DBObjectRights.rights)}.{nameof(DBObjectRightValue.value)}"));
        ;

        var indexModel = new CreateIndexModel<DBObjectRights>(
          keys, new CreateIndexOptions()
          { Name = "roles" }
        );

        _coll.Indexes.CreateOneAsync(indexModel);
      }
    }

    async Task<long> IRightService.DeleteAsync(string id)
    {
      var result = await _coll.DeleteOneAsync(
          x => x.id == id);
      return result.DeletedCount;
    }

    async Task<Dictionary<string, ObjectRightsDTO>> IRightService.GetListByIdsAsync(
      List<string> ids
    )
    {
      List<DBObjectRights> obj = null;

      try
      {
        obj = await _coll.Find(s => ids.Contains(s.id)).ToListAsync();
      }
      catch (Exception)
      {

      }

      return ConvertListStateDB2DTO(obj);
    }

    async Task<long> IRightService.UpdateListAsync(List<ObjectRightsDTO> newObjs)
    {
      var dbUpdated = new Dictionary<ObjectRightsDTO, DBObjectRights>();
      var bulkWrites = new List<WriteModel<DBObjectRights>>();

      foreach (var item in newObjs)
      {
        var updatedObj = ConvertRightDTO2DB(item);
        dbUpdated.Add(item, updatedObj);
        var filter = Builders<DBObjectRights>.Filter.Eq(x => x.id, updatedObj.id);

        if (string.IsNullOrEmpty(updatedObj.id))
        {
          var request = new InsertOneModel<DBObjectRights>(updatedObj);
          bulkWrites.Add(request);
        }
        else
        {
          var request = new ReplaceOneModel<DBObjectRights>(filter, updatedObj);
          request.IsUpsert = true;
          bulkWrites.Add(request);
        }
      }

      var writeResult = await _coll.BulkWriteAsync(bulkWrites);

      foreach (var pair in dbUpdated)
      {
        pair.Key.id = pair.Value.id;
      }
      return writeResult.ModifiedCount;
    }

    DBObjectRights ConvertRightDTO2DB(ObjectRightsDTO dto)
    {
      if (dto == null)
      {
        return null;
      }

      var dbo = new DBObjectRights()
      {
        id = dto.id,
        rights = new List<DBObjectRightValue>()
      };

      foreach (var item in dto.rights)
      {
        dbo.rights.Add(new DBObjectRightValue()
        {
          role = item.role,
          value = (int)item.value
        });
      }

      return dbo;
    }

    Dictionary<string, ObjectRightsDTO> ConvertListStateDB2DTO(List<DBObjectRights> dbo)
    {
      if (dbo == null)
      {
        return null;
      }

      var newObjs = new Dictionary<string, ObjectRightsDTO>();

      foreach (var dbObj in dbo)
      {
        var dto = new ObjectRightsDTO()
        {
          id = dbObj.id,
          rights = new List<ObjectRightValueDTO>()
        };

        foreach (var item in dbObj.rights)
        {
          dto.rights.Add(new ObjectRightValueDTO()
          {
            role = item.role,
            value = (ObjectRightValueDTO.ERightValue)item.value
          });
        }

        newObjs.Add(dto.id, dto);
      }
      return newObjs;
    }
  }
}
