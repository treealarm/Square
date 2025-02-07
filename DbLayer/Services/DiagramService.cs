using DbLayer.Models;
using Domain;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DbLayer.Services
{
  internal class DiagramService : IDiagramService, IDiagramServiceInternal
  {
    private readonly IMongoCollection<DBDiagram> _coll;
    private readonly IMongoClient _mongoClient;
    public DiagramService(
      IOptions<MapDatabaseSettings> geoStoreDatabaseSettings,
      IMongoClient mongoClient)
    {
      _mongoClient = mongoClient;

      var mongoDatabase = _mongoClient.GetDatabase(
          geoStoreDatabaseSettings.Value.DatabaseName);

      _coll =
        mongoDatabase.GetCollection<DBDiagram>
        (geoStoreDatabaseSettings.Value.DiagramCollectionName);

      CreateIndexes();
    }

    private void CreateIndexes()
    {
    }

    async Task IDiagramServiceInternal.RemoveAsync(List<string> ids)
    {
      await _coll.DeleteManyAsync(
          x => ids.Contains(x.id));
    }

    async Task<Dictionary<string, DiagramDTO>> IDiagramService.GetListByIdsAsync(
      List<string> ids
    )
    {
      List<DBDiagram> obj = null;

      try
      {
        obj = await _coll.Find(s => ids.Contains(s.id)).ToListAsync();
      }
      catch (Exception)
      {

      }

      return ConvertListDB2DTO(obj);
    }

    async Task IDiagramServiceInternal.UpdateListAsync(List<DiagramDTO> newObjs) 
    {
      if (newObjs.Count == 0)
      {
        return;
      }
      var dbUpdated = new Dictionary<DiagramDTO, DBDiagram>();
      var bulkWrites = new List<WriteModel<DBDiagram>>();

      foreach (var item in newObjs)
      {
        var updatedObj = ConvertDTO2DB(item);
        dbUpdated.Add(item, updatedObj);
        var filter = Builders<DBDiagram>.Filter.Eq(x => x.id, updatedObj.id);

        if (string.IsNullOrEmpty(updatedObj.id))
        {
          var request = new InsertOneModel<DBDiagram>(updatedObj);
          bulkWrites.Add(request);
        }
        else
        {
          var request = new ReplaceOneModel<DBDiagram>(filter, updatedObj);
          request.IsUpsert = true;
          bulkWrites.Add(request);
        }
      }

      var writeResult = await _coll.BulkWriteAsync(bulkWrites);

      foreach (var pair in dbUpdated)
      {
        pair.Key.id = pair.Value.id;
      }
    }

    DBDiagram ConvertDTO2DB(DiagramDTO dto)
    {
      if (dto == null)
      {
        return null;
      }

      var dbo = new DBDiagram()
      {
        id = dto.id,
        dgr_type = dto.dgr_type,
        region_id = dto.region_id,
        background_img = dto.background_img,
        geometry = new DBDiagramCoord(),
      };

      dto.geometry?.CopyAllTo(dbo.geometry);
      return dbo;
    }

    Dictionary<string, DiagramDTO> ConvertListDB2DTO(List<DBDiagram> dbo)
    {
      if (dbo == null)
      {
        return null;
      }

      var newObjs = new Dictionary<string, DiagramDTO>();

      foreach (var dbObj in dbo)
      {
        var dto = new DiagramDTO()
        {
          id = dbObj.id,
          dgr_type = dbObj.dgr_type,
          geometry = new DiagramCoordDTO(),
          background_img = dbObj.background_img,
          region_id = dbObj.region_id
        };

        dbObj.geometry?.CopyAllTo(dto.geometry);

        newObjs.Add(dto.id, dto);
      }
      return newObjs;
    }
  }
}
