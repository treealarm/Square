﻿using DbLayer.Models;
using Domain;
using Domain.ServiceInterfaces;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbLayer.Services
{
    public class LevelService : ILevelService
    {
        private readonly IMongoCollection<DBLevel> _collection;
        private readonly MongoClient _mongoClient;
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

        public LevelService(IOptions<MapDatabaseSettings> geoStoreDatabaseSettings)
        {
            _mongoClient = new MongoClient(
              geoStoreDatabaseSettings.Value.ConnectionString);

            var mongoDatabase = _mongoClient.GetDatabase(
                geoStoreDatabaseSettings.Value.DatabaseName);

            _collection =
              mongoDatabase.GetCollection<DBLevel>(geoStoreDatabaseSettings.Value.LevelCollectionName);
        }

        public async Task InsertManyAsync(List<DBLevel> newObjs)
        {
            await _collection.InsertManyAsync(newObjs);
        }

        public async Task<List<DBLevel>> GetLevelsAsync()
        {
            List<DBLevel> obj = null;

            try
            {
                obj = await _collection.AsQueryable().ToListAsync();
            }
            catch (Exception)
            {

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

          if (_cash.Count == 0)
          {
              List<DBLevel> levels = new List<DBLevel>();

              for (int i = 0; i < 20; i++)
              {
                  DBLevel level = new DBLevel()
                  {
                      zoom_level = i.ToString(),
                      zoom_min = i,
                      zoom_max = i
                  };

                  if (i == 13)
                  {
                      level.zoom_min = 10;
                      level.zoom_max = 16;
                  }
                  levels.Add(level);
              }

              await InsertManyAsync(levels);
              _cash = await GetLevelsAsync();
          }
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