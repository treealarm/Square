using DbLayer.Models.Diagrams;
using Domain;
using Domain.Diagram;
using Domain.OptionsModels;
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
  public class DiagramService : IDiagramService
  {
    private readonly IMongoCollection<DBDiagram> _coll;
    private readonly MongoClient _mongoClient;
    public DiagramService(IOptions<MapDatabaseSettings> geoStoreDatabaseSettings)
    {
      _mongoClient = new MongoClient(
        geoStoreDatabaseSettings.Value.ConnectionString);

      var mongoDatabase = _mongoClient.GetDatabase(
          geoStoreDatabaseSettings.Value.DatabaseName);

      _coll =
        mongoDatabase.GetCollection<DBDiagram>
        (geoStoreDatabaseSettings.Value.DiagramCollectionName);

      CreateIndexes();
    }

    private void CreateIndexes()
    {
      {
        //IndexKeysDefinition<DBDiagram> keys =
        //        new IndexKeysDefinitionBuilder<DBDiagram>()
        //        .Ascending(d => d.geometry)
        //        ;

        //var indexModel = new CreateIndexModel<DBDiagram>(
        //  keys, new CreateIndexOptions()
        //  { Name = "geometry" }
        //);

        //_coll.Indexes.CreateOneAsync(indexModel);
      }
    }

    async Task IDiagramService.DeleteAsync(string id)
    {
      await _coll.DeleteOneAsync(
          x => x.id == id);
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

    async Task IDiagramService.UpdateListAsync(List<DiagramDTO> newObjs)
    {
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
          geometry = new DiagramCoordDTO()
        };

        dbObj.geometry?.CopyAllTo(dto.geometry);

        newObjs.Add(dto.id, dto);
      }
      return newObjs;
    }
  }
}
