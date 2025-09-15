using DbLayer.Models;
using Domain;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace DbLayer.Services
{
  internal class LevelService : ILevelService
  {
    private readonly PgDbContext _db;
    private static List<DBLevel> _cache = null;

    public LevelService(PgDbContext db)
    {
      _db = db;
    }

    private async Task<List<DBLevel>> GetCache()
    {
      if (_cache == null)
        await RebuildCache();
      return _cache;
    }

    public async Task Init()
    {
      await RebuildCache();
    }

    public async Task RebuildCache()
    {
      _cache = await _db.Levels.AsNoTracking().ToListAsync();

      if (_cache?.Count == 0)
      {
        List<DBLevel> levels = new List<DBLevel>();

        for (int j = 1; j < 20; j++)
        {
          for (int i = j; i < 20; i++)
          {
            var guid = Guid.NewGuid();
            var str_guid = Domain.Utils.ConvertGuidToObjectId(guid);
            guid = Domain.Utils.ConvertObjectIdToGuid(str_guid) ?? guid;

            var levelName = i == j ? i.ToString() : $"{j}-{i}";
            levels.Add(new DBLevel
            {
              id = guid,
              zoom_level = levelName,
              zoom_min = j,
              zoom_max = i
            });
          }
        }

        await _db.Levels.AddRangeAsync(levels);
        await _db.SaveChangesAsync();
        _cache = await _db.Levels.AsNoTracking().ToListAsync();
      }
    }

    public async Task<LevelDTO> GetByZoomLevel(string name)
    {
      var cache = await GetCache();
      var result = cache.FirstOrDefault(l => l.zoom_level == name);

      if (result == null)
        return null;

      var dto = new LevelDTO();
      result.CopyAllTo(dto);
      return dto;
    }

    public async Task<Dictionary<string, LevelDTO>> GetAllZooms()
    {
      var cache = await GetCache();
      return cache.ToDictionary(
        c => c.zoom_level,
        c => { var dto = new LevelDTO(); c.CopyAllTo(dto); return dto; }
      );
    }

    public async Task<List<string>> GetLevelsByZoom(double? zoom)
    {
      var cache = await GetCache();
      if (zoom == null)
        return new List<string>();

      return cache
        .Where(l => zoom <= l.zoom_max && zoom >= l.zoom_min)
        .Select(l => l.zoom_level)
        .ToList();
    }
  }
}
