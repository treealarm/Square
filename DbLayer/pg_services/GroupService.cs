using DbLayer.Models;
using Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DbLayer.Services
{
  internal class GroupsService : IGroupsService, IIGroupsServiceInternal
  {
    private readonly PgDbContext _dbContext;

    public GroupsService(
      PgDbContext context)
    {
      _dbContext = context;
    }

    public static Dictionary<string, GroupDTO> ConvertListDB2DTO(List<DBGroup> dbObjs)
    {
      var result = new Dictionary<string, GroupDTO>();

      foreach (var dbItem in dbObjs)
      {
        result.Add(Domain.Utils.ConvertGuidToObjectId(dbItem.id), ConvertDB2DTO(dbItem)!);
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
        id = Domain.Utils.ConvertGuidToObjectId(dbo.id),
        name = dbo.name,
        objid = Domain.Utils.ConvertGuidToObjectId(dbo.objid)
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
        id = Domain.Utils.ConvertObjectIdToGuid(dto.id)??Guid.Empty,
        name = dto.name,
        objid = Domain.Utils.ConvertObjectIdToGuid(dto.objid) ?? Guid.Empty
      };

      return dbo;
    }

    public async Task<Dictionary<string, GroupDTO>> GetListByIdsAsync(List<string> ids)
    {
      if (ids == null || ids.Count == 0)
        return new Dictionary<string, GroupDTO>();

      // Конвертируем строки в Guid, отбрасываем некорректные
      List<Guid> guidIds = ids
          .Select(s => Domain.Utils.ConvertObjectIdToGuid(s))
          .Where(g => g.HasValue)
          .Select(g => g.Value)
          .ToList();

      if (guidIds.Count == 0)
        return new Dictionary<string, GroupDTO>();

      // Фильтруем по чистому списку Guid
      var groups = await _dbContext.Groups
                                   .Where(g => guidIds.Contains(g.id))
                                   .ToListAsync();

      return GroupsService.ConvertListDB2DTO(groups);
    }



    public async Task RemoveAsync(List<string> ids)
    {
      if (ids == null || ids.Count == 0)
        return;

      // Конвертируем строки в Guid, отбрасываем некорректные
      List<Guid> guidIds = ids
          .Select(s => Domain.Utils.ConvertObjectIdToGuid(s))
          .Where(g => g.HasValue)
          .Select(g => g.Value)
          .ToList();

      if (guidIds.Count == 0)
        return;

      // Находим сущности для удаления
      var groupsToRemove = await _dbContext.Groups
                                           .Where(g => guidIds.Contains(g.id))
                                           .ToListAsync();

      if (groupsToRemove.Count == 0)
        return;

      _dbContext.Groups.RemoveRange(groupsToRemove);
      await _dbContext.SaveChangesAsync();
    }


    public async Task UpdateListAsync(List<GroupDTO> valuesToUpdate)
    {
      if (valuesToUpdate == null || valuesToUpdate.Count == 0)
        return;

      // Преобразуем DTO в Guid и готовим список
      var dtoIdPairs = valuesToUpdate
          .Select(dto =>
          {
            Guid id = Domain.Utils.ConvertObjectIdToGuid(dto.id) ?? Domain.Utils.NewGuid();
            return new { Id = id, Dto = dto };
          })
          .ToList();

      var ids = dtoIdPairs.Select(x => x.Id).ToList();

      // Загружаем существующие группы
      var existingGroups = await _dbContext.Groups
                                           .Where(g => ids.Contains(g.id))
                                           .ToListAsync();

      // Словарь для быстрого поиска
      var existingGroupsDict = existingGroups.ToDictionary(g => g.id, g => g);

      foreach (var pair in dtoIdPairs)
      {
        if (existingGroupsDict.TryGetValue(pair.Id, out var existingGroup))
        {
          // Обновляем существующую запись
          existingGroup.name = pair.Dto.name;
          existingGroup.objid = Domain.Utils.ConvertObjectIdToGuid(pair.Dto.objid) ?? Guid.Empty;
        }
        else
        {
          // Создаём новую запись
          var newGroup = new DBGroup
          {
            id = pair.Id,
            name = pair.Dto.name,
            objid = Domain.Utils.ConvertObjectIdToGuid(pair.Dto.objid) ?? Guid.Empty
          };
          _dbContext.Groups.Add(newGroup);

          // Сохраняем id обратно в DTO, чтобы оставалось совместимо с предыдущим кодом
          pair.Dto.id = newGroup.id.ToString();
        }
      }

      await _dbContext.SaveChangesAsync();
    }



    public async Task<Dictionary<string, GroupDTO>> GetListByNamesAsync(List<string> names)
    {
      if (names == null || names.Count == 0)
        return new Dictionary<string, GroupDTO>();

      var groups = await _dbContext.Groups
                                   .Where(g => names.Contains(g.name))
                                   .ToListAsync();

      return GroupsService.ConvertListDB2DTO(groups);
    }


    public async Task RemoveByNameAsync(List<string> names)
    {
      if (names == null || names.Count == 0)
        return;

      // Находим группы для удаления
      var groupsToRemove = await _dbContext.Groups
                                           .Where(g => names.Contains(g.name))
                                           .ToListAsync();

      if (groupsToRemove.Count == 0)
        return;

      _dbContext.Groups.RemoveRange(groupsToRemove);
      await _dbContext.SaveChangesAsync();
    }

  }
}
