using DbLayer.Models;
using Domain;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace DbLayer.Services
{
  internal class IntegroService : IIntegroService, IIntegroServiceInternal,
    IIntegroTypesService, IIntegroTypesInternal
  {
    private readonly PgDbContext _dbContext;


    public IntegroService(
      PgDbContext context)
    {
      _dbContext = context;
    }

    internal static Expression<Func<DBIntegroType, bool>> BuildFilter(IEnumerable<IntegroTypeKeyDTO> types)
    {
      var predicate = PredicateBuilder.False<DBIntegroType>();

      foreach (var t in types)
      {
        // Пропускаем полностью пустые ключи
        if (string.IsNullOrEmpty(t.i_type) && string.IsNullOrEmpty(t.i_name))
          continue;

        Expression<Func<DBIntegroType, bool>> condition = x => true;

        if (!string.IsNullOrEmpty(t.i_type))
        {
          Expression<Func<DBIntegroType, bool>> typeMatch = x => x.i_type == t.i_type;
          condition = condition.And(typeMatch);
        }

        if (!string.IsNullOrEmpty(t.i_name))
        {
          Expression<Func<DBIntegroType, bool>> nameMatch = x => x.i_name == t.i_name;
          condition = condition.And(nameMatch);
        }

        predicate = predicate.Or(condition);
      }

      return predicate;
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
              Utils.GenerateId24().ToString()) ?? throw new InvalidOperationException("ConvertObjectIdToGuid");
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
        id = Utils.ConvertObjectIdToGuid(dto.id) ?? throw new InvalidOperationException("ConvertObjectIdToGuid"),
        i_name = dto.i_name,
        i_type = dto.i_type
      };
      return dbo;
    }
    private IntegroDTO ConverDB2DTO(DBIntegro o_in)
    {
      var o_out = new IntegroDTO()
      {
        id = Utils.ConvertGuidToObjectId(o_in.id),
        i_name = o_in.i_name,
        i_type = o_in.i_type
      };
      return o_out;
    }

    public Dictionary<string, IntegroDTO> ConvertListDB2DTO(List<DBIntegro> dbObjs)
    {
      var result = new Dictionary<string, IntegroDTO>();

      foreach (var dbItem in dbObjs)
      {
        var dto = ConverDB2DTO(dbItem);
        result.Add(dto.id, dto);
      }

      return result;
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
      var result = ConvertListDB2DTO(dbObjs);
      return result;
    }
    public async Task<Dictionary<string, IntegroDTO>> GetListByType(string i_name, string i_type)
    {
      var query = _dbContext.Integro.AsQueryable();

      query = query.Where(i => i.i_name == i_name);

      if (!string.IsNullOrEmpty(i_type))
      {
        query = query.Where(i => i.i_type == i_type);
      }

      var dbObjs = await query.ToListAsync();

      return ConvertListDB2DTO(dbObjs);
    }

    public async Task<Dictionary<string, IntegroTypeDTO>> GetTypesAsync(List<IntegroTypeKeyDTO> types)
    {
      var filter = BuildFilter(types);

      var dbObjs = await _dbContext.IntegroTypes
        .Where(filter)
        .Include(t => t.children)
        .ToListAsync();

      var result = new Dictionary<string, IntegroTypeDTO>();

      foreach (var dbItem in dbObjs)
      {
        var dto = new IntegroTypeDTO()
        {
          i_type = dbItem.i_type,
          i_name = dbItem.i_name,
        };

        foreach (var i in dbItem.children)
        {
          dto.children.Add(new IntegroTypeChildDTO()
          {
            child_i_type = i.child_i_type
          });
        }
        result.Add(dbItem.i_type, dto);
      }

      return result;
    }

    async Task IIntegroTypesInternal.UpdateTypesAsync(List<IntegroTypeDTO> types)
    {
      IEnumerable<IntegroTypeKeyDTO> keyList = types;

      var filter = BuildFilter(keyList);

      var dbObjs = await _dbContext.IntegroTypes
        .Where(filter)
        .Include(t => t.children)
        .ToListAsync();


      var existingEntities = dbObjs.ToDictionary(e => e.i_type);

      var toUpdate = new List<DBIntegroType>();
      var toAdd = new List<DBIntegroType>();

      foreach (var Item in types)
      {
        List<DBIntegroTypeChild> children = new List<DBIntegroTypeChild> ();
        foreach (var child in Item.children)
        {
          // Добавляем новые дочерние элементы
          children.Add(new DBIntegroTypeChild
          {
            child_i_type = child.child_i_type,
            i_type = Item.i_type,
            i_name = Item.i_name
          });
        }

        if (existingEntities.TryGetValue(Item.i_type, out var existing))
        {
          existing.children.Clear(); // Очистить старые дочерние элементы

          existing.children.AddRange(children);

          toUpdate.Add(existing); // Добавляем в список для обновления
        }
        else
        {
          var dbo = new DBIntegroType()
          {
            i_type = Item.i_type,
            i_name = Item.i_name,
          };

          dbo.children.AddRange(children);
          toAdd.Add(dbo);
        }
      }

      _dbContext.IntegroTypes.UpdateRange(toUpdate);
      await _dbContext.IntegroTypes.AddRangeAsync(toAdd);
      await _dbContext.SaveChangesAsync();
    }

    async Task IIntegroTypesInternal.RemoveTypesAsync(List<IntegroTypeKeyDTO> types)
    {
      var filter = BuildFilter(types);

      var dbObjs = await _dbContext.IntegroTypes
        .Where(filter)
        .Include(t => t.children)
        .ToListAsync();

      _dbContext.IntegroTypes.RemoveRange(dbObjs);
      await _dbContext.SaveChangesAsync();

    }
  }
}
