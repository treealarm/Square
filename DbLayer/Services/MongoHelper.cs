using DbLayer.Models;
using Domain;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DbLayer.Services
{
  public static class MongoHelper
  {
    // Метод для получения данных по списку ID
    internal static async Task<Dictionary<string, T_DTO>> GetListByIdsAsync<T_DB, T_DTO>(
        IMongoCollection<T_DB> collection,
        List<string> ids,
        Func<List<T_DB>, Dictionary<string, T_DTO>> convertListFunc)
        where T_DB : BaseEntity
        where T_DTO : BaseObjectDTO
    {
      ids.RemoveAll(i => string.IsNullOrEmpty(i));
      var dbObjs = await collection.Find(s => ids.Contains(s.id)).ToListAsync();

      return convertListFunc(dbObjs);
    }

    // Метод для удаления данных по списку ID
    internal static async Task RemoveAsync<T_DB>(IMongoCollection<T_DB> collection, List<string> ids)
        where T_DB : BaseEntity
    {
        await collection.DeleteManyAsync(
           x => ids.Contains(x.id));
    }

    // Метод для обновления списка данных
    internal static async Task UpdateListAsync<T_DB, T_DTO>(
        IMongoCollection<T_DB> collection,
        List<T_DTO> obj2UpdateIn,
        Func<T_DTO, T_DB> convertDtoToDbFunc)
        where T_DB : BaseEntity
        where T_DTO : BaseObjectDTO
    {
      if (obj2UpdateIn.Count == 0)
      {
        return;
      }
      var dbUpdated = new Dictionary<T_DTO, T_DB>();

      var bulkWrites = new List<WriteModel<T_DB>>();

      foreach (var item in obj2UpdateIn)
      {
        var updatedObj = convertDtoToDbFunc(item);
        dbUpdated.Add(item, updatedObj);

        var filter = Builders<T_DB>.Filter.Eq(x => x.id, updatedObj.id);

        if (string.IsNullOrEmpty(updatedObj.id))
        {
          var request = new InsertOneModel<T_DB>(updatedObj);
          bulkWrites.Add(request);
        }
        else
        {
          var request = new ReplaceOneModel<T_DB>(filter, updatedObj) { IsUpsert = true };
          bulkWrites.Add(request);
        }
      }

        await collection.BulkWriteAsync(bulkWrites);

        foreach (var pair in dbUpdated)
        {
          pair.Key.id = pair.Value.id;
        }
      }
  }
}
