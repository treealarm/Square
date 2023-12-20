using DbLayer.Models;
using DbLayer.Models.Diagrams;
using Domain;
using Domain.Diagram;
using Domain.OptionsModels;
using Domain.Rights;
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
  public class DiagramTypeService: IDiagramTypeService
  {
    private readonly IMongoCollection<DBDiagramType> _coll;
    private readonly MongoClient _mongoClient;
    public DiagramTypeService(IOptions<MapDatabaseSettings> geoStoreDatabaseSettings)
    {
      _mongoClient = new MongoClient(
        geoStoreDatabaseSettings.Value.ConnectionString);

      var mongoDatabase = _mongoClient.GetDatabase(
          geoStoreDatabaseSettings.Value.DatabaseName);

      _coll =
        mongoDatabase.GetCollection<DBDiagramType>
        (geoStoreDatabaseSettings.Value.DiagramTypeCollectionName);

      CreateIndexes();
    }

    private void CreateIndexes()
    {
      {
        IndexKeysDefinition<DBDiagramType> keys =
                new IndexKeysDefinitionBuilder<DBDiagramType>()
                .Ascending(d => d.name)
                ;

        var indexModel = new CreateIndexModel<DBDiagramType>(
          keys, new CreateIndexOptions()
          { Name = "type_name" }
        );

        _coll.Indexes.CreateOneAsync(indexModel);
      }
    }

    async Task IDiagramTypeService.DeleteAsync(string id)
    {
      await _coll.DeleteOneAsync(
          x => x.id == id);
    }

    async Task<Dictionary<string, DiagramTypeDTO>> IDiagramTypeService.GetListByTypeNamesAsync(
      List<string> typeNames
    )
    {
      List<DBDiagramType> obj = null;

      try
      {
        obj = await _coll.Find(s => typeNames.Contains(s.name)).ToListAsync();
      }
      catch (Exception)
      {

      }

      return ConvertListDB2DTO(obj);
    }

    async Task IDiagramTypeService.UpdateListAsync(List<DiagramTypeDTO> newObjs)
    {
      var dbUpdated = new Dictionary<DiagramTypeDTO, DBDiagramType>();
      var bulkWrites = new List<WriteModel<DBDiagramType>>();

      foreach (var item in newObjs)
      {
        var updatedObj = ConvertDTO2DB(item);
        dbUpdated.Add(item, updatedObj);
        var filter = Builders<DBDiagramType>.Filter.Eq(x => x.id, updatedObj.id);

        if (string.IsNullOrEmpty(updatedObj.id))
        {
          var request = new InsertOneModel<DBDiagramType>(updatedObj);
          bulkWrites.Add(request);
        }
        else
        {
          var request = new ReplaceOneModel<DBDiagramType>(filter, updatedObj);
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

    DBDiagramType ConvertDTO2DB(DiagramTypeDTO dto)
    {
      if (dto == null)
      {
        return null;
      }

      var dbo = new DBDiagramType()
      {
        id = dto.id,
        regions = new List<DBDiagramTypeRegion>(),
        src = dto.src,
        name = dto.name
      };

      foreach (var item in dto.regions)
      {
        dbo.regions.Add(new DBDiagramTypeRegion() 
        {
          geometry = item.geometry.CopyAll<DiagramCoordDTO, DBDiagramCoord>(),
          id = item.id
        });
      }

      return dbo;
    }

    Dictionary<string, DiagramTypeDTO> ConvertListDB2DTO(List<DBDiagramType> dbo)
    {
      if (dbo == null)
      {
        return null;
      }

      var newObjs = new Dictionary<string, DiagramTypeDTO>();

      foreach (var dbObj in dbo)
      {
        var dto = new DiagramTypeDTO()
        {
          id = dbObj.id,
          regions = new List<DiagramTypeRegionDTO>(),
          src = dbObj.src,
          name = dbObj.name,
        };

        foreach (var item in dbObj.regions)
        {
          dto.regions.Add(new DiagramTypeRegionDTO() 
          {
             id= item.id,
             geometry = item.geometry.CopyAll<DBDiagramCoord, DiagramCoordDTO>()
          });
        }

        newObjs.Add(dto.id, dto);
      }
      return newObjs;
    }
  }
}
