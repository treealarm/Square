using DbLayer.Models;
using Domain;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DbLayer.Services
{
  internal class IntegroService : IIntegroService, IIntegroServiceInternal
  {
    private readonly PgDbContext _dbContext;


    public IntegroService(
      PgDbContext context)
    {
      _dbContext = context;
    }

    internal static async Task UpdateListAsync<T_DB, T_DTO>(
        DbSet<T_DB> dbSet,
        List<T_DTO> obj2UpdateIn,
        Func<T_DTO, T_DB> convertDtoToDbFunc,
        DbContext dbContext)
        where T_DB : BasePgEntity
        where T_DTO : BaseObjectDTO
    {
      if (obj2UpdateIn.Count == 0)
      {
        return;
      }

      // Список id, которые нам нужно обновить или добавить
      var idsToUpdate = obj2UpdateIn
        .Where(i=> !string.IsNullOrEmpty(i.id))
        .Select(item => Utils.ConvertObjectIdToGuid(item.id)).ToList();

      // 1. Извлекаем все объекты, которые уже есть в базе
      var existingEntities = await dbSet
          .Where(entity => idsToUpdate.Contains(entity.id))
          .ToListAsync();
      var entityDictionary = existingEntities.ToDictionary(e => e.id);

      // 2. Создаем список для обновления и добавления
      var toUpdate = new List<T_DB>();
      var toAdd = new List<T_DB>();

      // Разделяем на обновление и добавление
      foreach (var item in obj2UpdateIn)
      {
        var updatedObj = convertDtoToDbFunc(item);

        if (entityDictionary.TryGetValue(updatedObj.id, out var existingObj))
        {
          // Если сущность существует, добавляем в список для обновления
          dbContext.Entry(existingObj).CurrentValues.SetValues(updatedObj);
          toUpdate.Add(existingObj);
        }
        else
        {
          // Если сущности нет, добавляем в список для добавления
          if (updatedObj.id == Guid.Empty)
          {
            updatedObj.id = Utils.ConvertObjectIdToGuid(
              Utils.GenerateId24().ToString());
          }
          toAdd.Add(updatedObj);
        }
      }

      dbSet.UpdateRange(toUpdate);
      await dbSet.AddRangeAsync(toAdd);

      // Сохраняем изменения в базе данных
      await dbContext.SaveChangesAsync();
    }

    private DBIntegro ConvertDTO2DB(IntegroDTO dto)
    {
      DBIntegro dbo = new DBIntegro()
      {
        id = Utils.ConvertObjectIdToGuid(dto.id),
        i_name = dto.i_name,
        i_type = dto.i_type
      };
      return dbo;
    }
    public async Task UpdateListAsync(List<IntegroDTO> obj2UpdateIn)
    {      
      if (obj2UpdateIn.Where(i => string.IsNullOrEmpty(i.i_name) ||
      string.IsNullOrEmpty(i.id)).Any())
      {
        throw new Exception("id and i_name must not be empty for integro");
      }

      await UpdateListAsync(
         _dbContext.Integro,
         obj2UpdateIn,
        ConvertDTO2DB,
        _dbContext);

      await _dbContext.SaveChangesAsync();
    }

    public async Task RemoveAsync(List<string> ids)
    {
      // Преобразуем все ids в Guid
      var guids = ids.Select(id => Utils.ConvertObjectIdToGuid(id)).ToList();

      // Удаляем объекты с соответствующими id (Guid)
      await _dbContext.Integro
          .Where(i => guids.Contains(i.id))
          .ExecuteDeleteAsync();
    }


    public async Task<Dictionary<string, IntegroDTO>> GetListByIdsAsync(List<string> ids)
    {
      var guids = ids.Select(id => Utils.ConvertObjectIdToGuid(id)).ToList();

      var dbObjs = await _dbContext.Integro.Where(i => guids.Contains(i.id)).ToListAsync();
      var result = PropertyCopy.ConvertListDB2DTO<DBIntegro, IntegroDTO>(dbObjs);
      return result;
    }
    public async Task<Dictionary<string, IntegroDTO>> GetListByType(string i_name, string i_type)
    {
      var dbObjs = await _dbContext.Integro.Where(i => i.i_name == i_name && i.i_type == i_type).ToListAsync();
      return PropertyCopy.ConvertListDB2DTO<DBIntegro, IntegroDTO>(dbObjs); 
    }
  }
}
