using DbLayer.Models;
using Domain;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace DbLayer.Services
{
  internal class RightService : IRightServiceInternal
  {
    private readonly PgDbContext _db;

    public RightService(PgDbContext db)
    {
      _db = db;
    }

    async Task<long> IRightUpdateService.Delete(string id)
    {
      var guid = Domain.Utils.ConvertObjectIdToGuid(id);

      var rows = await _db.Rights
        .Where(r => r.object_id == guid)
        .ExecuteDeleteAsync();

      return rows;
    }

    async Task<Dictionary<string, List<ObjectRightValueDTO>>> IRightService.GetListByIdsAsync(List<string> ids)
    {
      var guids = ids
        .Select(id => Domain.Utils.ConvertObjectIdToGuid(id))
        .Where(g => g != null)
        .Select(g => g!.Value)
        .ToList();

      var rights = await _db.Rights
        .Where(r => guids.Contains(r.object_id))
        .ToListAsync();

      var grouped = rights
        .GroupBy(r => r.object_id.ToString())
        .ToDictionary(
          g => g.Key,
          g => g.Select(r => new ObjectRightValueDTO
          {
            object_id = g.Key,
            role = r.role,
            value = (ObjectRightValueDTO.ERightValue)r.value
          }).ToList()
        );

      return grouped;
    }

    async Task<List<ObjectRightValueDTO>> IRightUpdateService.Update(List<ObjectRightValueDTO> newObjs)
    {
      // 1. Получаем уникальные object_id
      var uniqueIds = newObjs
          .Select(r => Domain.Utils.ConvertObjectIdToGuid(r.object_id))
          .Where(g => g != null)
          .Select(g => g!.Value)
          .Distinct()
          .ToList();

      // 2. Удаляем старые права всех объектов одной командой
      await _db.Rights
          .Where(r => uniqueIds.Contains(r.object_id))
          .ExecuteDeleteAsync();

      // 3. Создаём новые права
      var newRights = newObjs
          .Where(r => Domain.Utils.ConvertObjectIdToGuid(r.object_id) != null)
          .Select(r => new DBObjectRightValue
          {
            object_id = Domain.Utils.ConvertObjectIdToGuid(r.object_id)!.Value,
            role = r.role,
            value = (int)r.value
          })
          .ToList();

      // 4. Добавляем их в контекст
      await _db.Rights.AddRangeAsync(newRights);
      await _db.SaveChangesAsync();

      return newObjs;
    }

  }
}
