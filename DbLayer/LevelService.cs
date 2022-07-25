using DbLayer.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbLayer
{
  public class LevelService
  {
    private readonly IMongoCollection<Level> _collection;
    private object _locker = new object();
    private List<Level> _cash = null;
    private async Task<List<Level>> GetCash()
    {
      if (_cash == null)
      {
        await RebuildCash();
      }
      return _cash;
    }
    public LevelService(IMongoCollection<Level> collection)
    {
      _collection = collection;
    }

    public async Task InsertManyAsync(List<Level> newObjs)
    {
      await _collection.InsertManyAsync(newObjs);
    }

    public async Task<List<Level>> GetLevelsAsync()
    {
      List<Level> obj = null;

      try
      {
        obj = await _collection.AsQueryable().ToListAsync();
      }
      catch (Exception)
      {

      }

      return obj;
    }

    public async Task RebuildCash()
    {
      _cash = await GetLevelsAsync();

      if (_cash.Count == 0)
      {
        List<Level> levels = new List<Level>();

        for (int i = 0; i < 20; i++)
        {
          Level level = new Level()
          {
            zoom_level = i.ToString(),
            zoom_min = i,
            zoom_max = i
          };
          levels.Add(level);
        }

        await InsertManyAsync(levels);
        _cash = await GetLevelsAsync();
      }
    }

    public async Task<List<string>> GetLevelsByZoom(double zoom)
    {
      var cash = await GetCash();

      var result = cash.Where(l => zoom <= l.zoom_max && zoom >= l.zoom_min)
        .Select(t => t.zoom_level)
        .ToList();
      return result;
    }
  }
}
