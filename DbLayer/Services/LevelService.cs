using DbLayer.Models;
using Domain;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DbLayer.Services
{
  internal class LevelService : ILevelService
  {
    private readonly IMongoCollection<DBLevel> _collection;
    private readonly IMongoClient _mongoClient;
    private object _locker = new object();
    private List<DBLevel> _cash = null;
    private async Task<List<DBLevel>> GetCash()
    {
      if (_cash == null)
      {
        await RebuildCash();
      }
      return _cash;
    }

    public LevelService(
      IOptions<MapDatabaseSettings> geoStoreDatabaseSettings,
      IMongoClient mongoClient)
    {
      _mongoClient = mongoClient;

      var mongoDatabase = _mongoClient.GetDatabase(
          geoStoreDatabaseSettings.Value.DatabaseName);

      _collection =
        mongoDatabase.GetCollection<DBLevel>(geoStoreDatabaseSettings.Value.LevelCollectionName);
    }

    private async Task InsertManyAsync(List<DBLevel> newObjs)
    {
      await _collection.InsertManyAsync(newObjs);
    }

    private async Task<List<DBLevel>> GetLevelsAsync()
    {
      List<DBLevel> obj = null;

      try
      {
        obj = await _collection.AsQueryable().ToListAsync();
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.ToString());
      }

      return obj;
    }

    public async Task Init()
    {
      await RebuildCash();
    }

    public async Task RebuildCash()
    {
      _cash = await GetLevelsAsync();

      if (_cash?.Count == 0)
      {
        List<DBLevel> levels = new List<DBLevel>();

        DBLevel level = new DBLevel()
        {
          zoom_level = "13-17",
          zoom_min = 13,
          zoom_max = 17
        };
        levels.Add(level);

        level = new DBLevel()
        {
          zoom_level = "14-17",
          zoom_min = 14,
          zoom_max = 17
        };

        levels.Add(level);
        for (int j = 1; j < 20; j++)
        {
          for (int i = j; i < 20; i++)
          {
            var levelName = $"{j}-{i}";

            if (i == j)
            {
              levelName = i.ToString();
            }
            level = new DBLevel()
            {
              zoom_level = levelName,
              zoom_min = j,
              zoom_max = i
            };
            levels.Add(level);
          }
        }
        

        await InsertManyAsync(levels);
        _cash = await GetLevelsAsync();
      }
    }

    public async Task<LevelDTO> GetByZoomLevel(string name)
    {
      var cash = await GetCash();

      var result = cash.Where(l => l.zoom_level == name)
        .FirstOrDefault();

      if (result == null)
      {
        return null;
      }

      LevelDTO dto = new LevelDTO();
      result.CopyAllTo(dto);

      return dto;
    }

    public async Task<Dictionary<string, LevelDTO>> GetAllZooms()
    {
      var cash = await GetCash();

      var retval = new Dictionary<string, LevelDTO>();

      foreach (var c in cash)
      {
        LevelDTO dto = new LevelDTO();
        c.CopyAllTo(dto);
        retval[c.zoom_level] = dto;
      }     

      return retval;
    }

    public async Task<List<string>> GetLevelsByZoom(double? zoom)
    {
      var cash = await GetCash();

      if (zoom == null)
      {
        return new List<string>();
      }

      var result = cash.Where(l => zoom <= l.zoom_max && zoom >= l.zoom_min)
        .Select(t => t.zoom_level)
        .ToList();
      return result;
    }
  }
}
