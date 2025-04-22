
using Domain;
using Google.Protobuf.WellKnownTypes;
using LeafletAlarms.Services;
using LeafletAlarmsGrpc;
using Common;
using Grpc.Core;

namespace LeafletAlarms.Grpc.Implementation
{
  public class GRPCServiceProxy
  {
    private readonly ITracksUpdateService _trackUpdateService;
    private readonly IStatesUpdateService _statesUpdateService;
    private readonly IEventsUpdateService _eventsUpdateService;
    private readonly IValuesUpdateService _valuesUpdateService;
    private readonly IDiagramTypeUpdateService _diagramTypeUpdateService;
    private readonly IDiagramUpdateService _diagramUpdateService;
    private readonly IIntegroUpdateService _integroUpdateService;
    private readonly FileSystemService _fs;
    private readonly IMapService _mapService;
    public GRPCServiceProxy(
      ITracksUpdateService trackUpdateService,
      IStatesUpdateService statesUpdateService,
      IEventsUpdateService eventsUpdateService,
      IValuesUpdateService valuesUpdateService,
      IDiagramTypeUpdateService diagramTypeUpdateService,
      IDiagramUpdateService diagramUpdateService,
      FileSystemService fs,
      IIntegroUpdateService integroUpdateService,
      IMapService mapService
    )
    {
      _trackUpdateService = trackUpdateService;
      _statesUpdateService = statesUpdateService;
      _eventsUpdateService = eventsUpdateService;
      _valuesUpdateService = valuesUpdateService;
      _diagramTypeUpdateService = diagramTypeUpdateService;
      _diagramUpdateService = diagramUpdateService;
      _integroUpdateService = integroUpdateService;
      _fs = fs;
      _mapService = mapService;
    }
    public static GeometryDTO CoordsFromProto2DTO(ProtoGeometry geometry)
    {
      GeometryDTO geo;

      if (geometry.Type == "Polygon" || geometry.Type == "LineString")
      {
        var polygonCoord = new GeometryPolygonDTO();
        geo = polygonCoord;
        polygonCoord.coord = new List<Geo2DCoordDTO>();

        foreach (var c in geometry.Coord)
        {
          polygonCoord.coord.Add(new Geo2DCoordDTO()
          {
            Lon = c.Lon,
            Lat = c.Lat
          });
        }
      }
      else
      if (geometry.Type == "Point")
      {
        var c = geometry.Coord.FirstOrDefault();

        if (c == null)
        {
          return null;
        }
        var pointCoord = new GeometryCircleDTO();
        geo = pointCoord;
        pointCoord.coord = new Geo2DCoordDTO()
        {
          Lon = c.Lon,
          Lat = c.Lat
        };
      }
      else
      {
        return null;
      }
      return geo;
    }

    public static ProtoGeometry ConvertGeoDTO2Proto(GeometryDTO location)
    {
      ProtoGeometry protoGeometry = new ProtoGeometry();

      if (location is GeometryCircleDTO point)
      {
        protoGeometry.Type = "Point";  // Устанавливаем тип
        protoGeometry.Coord.Add(
            new ProtoCoord { Lat = point.coord.Lat, Lon = point.coord.Lon });
      }

      if (location is GeometryPolygonDTO polygon)
      {
        protoGeometry.Type = "Polygon";  // Устанавливаем тип

        foreach (var coord in polygon.coord)
        {
          protoGeometry.Coord.Add(new ProtoCoord { Lat = coord.Lat, Lon = coord.Lon });
        }

        // Закрываем полигон, добавив первую координату в конец списка
        protoGeometry.Coord.Add(new ProtoCoord { Lat = polygon.coord[0].Lat, Lon = polygon.coord[0].Lon });
      }

      if (location is GeometryPolylineDTO line)
      {
        protoGeometry.Type = "LineString";  // Устанавливаем тип

        foreach (var coord in line.coord)
        {
          protoGeometry.Coord.Add(new ProtoCoord { Lat = coord.Lat, Lon = coord.Lon });
        }
      }

      return protoGeometry;
    }

    public async Task<ProtoFigures> UpdateFigures(ProtoFigures request)
    {
      ProtoFigures response = new ProtoFigures();

      //Console.WriteLine($"Received from:{request.ToString()}");
      var figs = new FiguresDTO();
      figs.figs = new List<FigureGeoDTO>();
      figs.add_tracks = request.AddTracks;

      foreach (var fig in request.Figs)
      {
        try
        {
          var newFigDto = new FigureGeoDTO();
          newFigDto.id = fig.Id;
          newFigDto.name = fig.Name;
          newFigDto.parent_id = fig.ParentId;
          newFigDto.radius = fig.Radius;
          newFigDto.zoom_level = fig.ZoomLevel;
          newFigDto.geometry = CoordsFromProto2DTO(fig.Geometry);

          if (newFigDto.geometry == null)
          {
            Console.Error.WriteLine($"{newFigDto.name} geometry == null");
            continue;
          }
          if (fig.ExtraProps != null)
          {
            newFigDto.extra_props = new List<ObjExtraPropertyDTO>();

            foreach (var e in fig.ExtraProps)
            {
              newFigDto.extra_props.Add(new ObjExtraPropertyDTO()
              {
                prop_name = e.PropName,
                str_val = e.StrVal,
                visual_type = e.VisualType
              });
            }
          }

          figs.figs.Add(newFigDto);
        }
        catch (Exception ex)
        {
          Console.WriteLine(ex.ToString());
        }
      }

      await _trackUpdateService.UpdateFigures(figs);

      foreach (var resFig in figs.figs)
      {
        var pFig = new ProtoFig()
        {
          Id = resFig.id,
          ParentId = resFig.parent_id,
          Name = resFig.name,
          Radius = resFig.radius.Value,
          ZoomLevel = resFig.zoom_level
        };

        foreach (var e in resFig.extra_props)
        {
          pFig.ExtraProps.Add(new ProtoObjExtraProperty()
          {
            PropName = e.prop_name,
            StrVal = e.str_val,
            VisualType = e.visual_type
          });
        }

        pFig.Geometry = new ProtoGeometry();

        foreach (var f in resFig.geometry.GetCoordArray())
        {
          pFig.Geometry.Coord.Add(new ProtoCoord()
          {
            Lat = f.Lat,
            Lon = f.Lon
          });
        }
        response.Figs.Add(pFig);
      }

      return response;
    }

    public async Task<ProtoObjPropsList> RequestProperties(ProtoObjectIds request)
    {
      var response = new ProtoObjPropsList();
      var objProps = await _mapService.GetPropsAsync(request.Ids.ToList());

      foreach (var objProp in objProps)
      {
        var protoProp = new ProtoObjProps()
        {
          Obj = new ProtoObject()
          {
            Id = objProp.Value.id,
            Name = objProp.Value.name,
            OwnerId = objProp.Value.owner_id,
            ParentId = objProp.Value.parent_id
          }
        };

        foreach(var prop in objProp.Value.extra_props)
        {
          protoProp.Properties.Add(new ProtoObjExtraProperty()
          {
            PropName = prop.prop_name,
            StrVal = prop.str_val,
            VisualType = prop.visual_type
          });
        }
        response.Objects.Add(protoProp);
      }
      return response;
    }
    public async Task<BoolValue> UpdateStates(ProtoObjectStates request)
    {
      var objStates = new List<ObjectStateDTO>();

      foreach (var state in request.States)
      {
        objStates.Add(new ObjectStateDTO()
        {
          id = state.Id,
          timestamp = state.Timestamp.ToDateTime(),
          states = state.States.ToList()
        });
      }

      var ret = new BoolValue();
      ret.Value = await _statesUpdateService.UpdateStates(objStates);

      return ret;
    }

    public async Task<BoolValue> UpdateTracks(TrackPointsProto request)
    {
      var ret = new BoolValue();
      ret.Value = false;

      var tracks = new List<TrackPointDTO>();

      foreach (var track in request.Tracks)
      {
        var newTrack = new TrackPointDTO()
        {
          id = track.Id,
          timestamp = track.Timestamp == null ? DateTime.UtcNow : track.Timestamp.ToDateTime(),
        };

        newTrack.figure = new GeoObjectDTO()
        {
          id = string.IsNullOrEmpty(track.Figure.Id) ? null : track.Figure.Id,
          radius = track.Figure.Radius,
          zoom_level = track.Figure.ZoomLevel,
          location = CoordsFromProto2DTO(track.Figure.Location)
        };

        if (newTrack.figure.location == null)
        {
          Console.WriteLine("Location Conversion failed");
          return ret;
        }

        if (track.ExtraProps != null)
        {
          newTrack.extra_props = new List<ObjExtraPropertyDTO>();

          foreach (var e in track.ExtraProps)
          {
            newTrack.extra_props.Add(new ObjExtraPropertyDTO()
            {
              prop_name = e.PropName,
              str_val = e.StrVal,
              visual_type = e.VisualType
            });
          }
        }

        tracks.Add(newTrack);
      }
      await _trackUpdateService.AddTracks(tracks);
      ret.Value = true;
      return ret;
    }
    private async Task<ObjExtraPropertyDTO> ProcessProperty(ProtoObjExtraProperty e)
    {
      if (e.VisualType == "base64image_fs")
      {
        var path = DateTime.UtcNow.ToString("yyyyMMdd");
        var fileName = Guid.NewGuid().ToString();
        const string mainFolder = "events";
        var bytes = Convert.FromBase64String(e.StrVal);
        await _fs.Upload(mainFolder, path, fileName, bytes);

        return new ObjExtraPropertyDTO()
        {
          prop_name = e.PropName,
          str_val = Path.Combine(mainFolder, path, fileName),
          visual_type = "image_fs"
        };
      }

      return new ObjExtraPropertyDTO()
      {
        prop_name = e.PropName,
        str_val = e.StrVal,
        visual_type = e.VisualType
      };
    }

    public async Task<BoolValue> UploadFile(UploadFileProto request)
    {      
      var path = await _fs.Upload(
        request.MainFolder, 
        request.Path, 
        request.FileName, 
        request.FileData.ToByteArray());

      return new BoolValue()
      {
        Value = !string.IsNullOrWhiteSpace(path)
      };
    }
    public async Task<BoolValue> UpdateEvents(EventsProto request)
    {
      var list = new List<EventDTO>();

      foreach (var ev in request.Events)
      {
        try
        {
          var pEvent = new EventDTO();

          pEvent.timestamp = ev.Timestamp.ToDateTime();

          pEvent.id = ev.Id;
          pEvent.object_id = ev.ObjectId;
          pEvent.event_name = ev.EventName;
          pEvent.event_priority = ev.EventPriority;
          pEvent.extra_props = new List<ObjExtraPropertyDTO>();

          foreach (var e in ev.ExtraProps)
          {
            try
            {
              var new_e = await ProcessProperty(e);
              pEvent.extra_props.Add(new_e);
            }
            catch (Exception ex)
            {
              Console.Error.WriteLine(ex.ToString());
            }
          }

          list.Add(pEvent);
        }
        catch (Exception ex)
        {
          Console.Error.WriteLine(ex.ToString());
        }
      }
      var count = await _eventsUpdateService.AddEvents(list);
      return new BoolValue() { Value = count == list.Count };
    }

    private object GetValueFromProto(ValueProtoType e)
    {
      if (e.HasIntValue)
      {
        return e.IntValue;
      }
      else if (e.HasStringValue)
      {
        return e.StringValue;
      }
      else if (e.HasDoubleValue)
      {
        return e.DoubleValue;
      }
      // Добавьте обработку других типов по необходимости

      return null; // Возвращаем null, если значение отсутствует или не поддерживается
    }

    private ValueProtoType SetValueToValueType(object value)
    {
      ValueProtoType valueType = new ValueProtoType();

      if (value is double doubleValue)
      {
        valueType.DoubleValue = doubleValue;
      }
      else if (value is int intValue)
      {
        valueType.IntValue = intValue;
      }
      else if (value is string stringValue)
      {
        valueType.StringValue = stringValue;
      }
      else
      {
        throw new ArgumentException("Unsupported value type");
      }

      return valueType;
    }

    public async Task<ValuesProto> UpdateValues(ValuesProto request)
    {
      ValuesProto response = new ValuesProto();

      var toUpdate = new List<ValueDTO>();

      foreach (var e in request.Values)
      {
        toUpdate.Add(new ValueDTO()
        {
          id = e.Id,
          name = e.Name,
          owner_id = e.OwnerId,
          value = GetValueFromProto(e.Value),
        });
      }
      var updated = await _valuesUpdateService.UpdateValuesFilteredByNameAsync(toUpdate);

      foreach (var e in updated.Values)
      {
        response.Values.Add(new ValueProto()
        {
          Id = e.id,
          OwnerId = e.owner_id,
          Name = e.name,
          Value = SetValueToValueType(e.value),
        });
      }

      return response;
    }

    public async Task<DiagramTypesProto> UpdateDiagramTypes(DiagramTypesProto request)
    {
      var response = new DiagramTypesProto();

      // Преобразуем request в список DiagramTypeDTO
      List<DiagramTypeDTO> dgr_types = request.DiagramTypes
          .Select(proto => new DiagramTypeDTO
          {
            id = proto.Id,
            name = proto.Name,
            src = proto.Src,
            regions = proto.Regions?.Select(region => new DiagramTypeRegionDTO
            {
              id = region.Id,
              geometry = region.Geometry != null ? new DiagramCoordDTO
              {
                top = region.Geometry.Top,
                left = region.Geometry.Left,
                width = region.Geometry.Width,
                height = region.Geometry.Height
              } : null,
              styles = region.Styles.ToDictionary()
            }).ToList()
          }).ToList();

      // Вызываем метод сервиса
      var updatedDiagramTypes = await _diagramTypeUpdateService.UpdateDiagramTypes(dgr_types);

      foreach (var dto in updatedDiagramTypes.dgr_types)
      {
        var regionsList = new List<DiagramTypeRegionProto>();

        if (dto.regions != null)
        {
          foreach (var region in dto.regions)
          {
            var diagramRegion = new DiagramTypeRegionProto
            {
              Id = region.id,
              Geometry = region.geometry != null ? new DiagramCoordProto
              {
                Top = region.geometry.top,
                Left = region.geometry.left,
                Width = region.geometry.width,
                Height = region.geometry.height
              } : null,
            
            };

            foreach (var pair in region.styles) 
            {
              diagramRegion.Styles.Add(pair.Key, pair.Value);
            }
            
            regionsList.Add(diagramRegion);
          }
        }

        var diagramTypeProto = new DiagramTypeProto
        {
          Id = dto.id,
          Name = dto.name,
          Src = dto.src,
        };

        foreach(var region in regionsList)
        {
          diagramTypeProto.Regions.Add(region);
        }
        
        response.DiagramTypes.Add(diagramTypeProto);
      }

      return response;
    }

    // Метод конвертации из Protobuf в DTO
    private DiagramDTO ConvertToDiagramDTO(DiagramProto proto)
    {
      return new DiagramDTO
      {
        id = proto.Id,
        geometry = proto.Geometry != null ? new DiagramCoordDTO
        {
          top = proto.Geometry.Top,
          left = proto.Geometry.Left,
          width = proto.Geometry.Width,
          height = proto.Geometry.Height
        } : null,
        region_id = proto.RegionId,
        dgr_type = proto.DgrType,
        background_img = proto.BackgroundImg
      };
    }

    // Метод конвертации из DTO в Protobuf
    private DiagramProto ConvertToDiagramProto(DiagramDTO dto)
    {
      return new DiagramProto
      {
        Id = dto.id,
        Geometry = dto.geometry != null ? new DiagramCoordProto
        {
          Top = dto.geometry.top,
          Left = dto.geometry.left,
          Width = dto.geometry.width,
          Height = dto.geometry.height
        } : null,
        RegionId = dto.region_id,
        DgrType = dto.dgr_type,
        BackgroundImg = dto.background_img
      };
    }

    // Функция для обновления диаграмм
    public async Task<DiagramsProto> UpdateDiagrams(DiagramsProto request)
    {
      var diagrams = request.Diagrams
          .Select(proto => ConvertToDiagramDTO(proto))
          .ToList();

      var updatedDiagrams = await _diagramUpdateService.UpdateDiagrams(diagrams);

      var response = new DiagramsProto();
      response.Diagrams.AddRange(updatedDiagrams.Select(dto => ConvertToDiagramProto(dto)));

      return response;
    }

    public async Task<ProtoObjectList> UpdateObjects(ProtoObjectList request)
    {
      var objects = new List<BaseMarkerDTO>();
      foreach (var obj in request.Objects) 
      {
        objects.Add(new BaseMarkerDTO()
        {
          id = obj.Id,
          name = obj.Name,
          owner_id = obj.OwnerId,
          parent_id = obj.ParentId
        });
      }
      await _mapService.UpdateHierarchyAsync(objects);

      var response = new ProtoObjectList();
      foreach (var obj in objects)
      {
        response.Objects.Add(new ProtoObject()
        {
          Id = obj.id,
          Name = obj.name,
          OwnerId = obj.owner_id,
          ParentId = obj.parent_id
        });
      }
      return response;
    }

    public async Task<ProtoObjectList> RequestObjects(ProtoObjectIds request)
    {
      var dic = await _mapService.GetAsync(request.Ids.ToList());

      var response = new ProtoObjectList();

      foreach (var obj in dic.Values)
      {
        var protoObject = new ProtoObject
        {
          Id = obj.id,
          Name = obj.name
        };

        if (obj.owner_id != null)
          protoObject.OwnerId = obj.owner_id;

        if (obj.parent_id != null)
          protoObject.ParentId = obj.parent_id;

        response.Objects.Add(protoObject);
      }
      return response;
    }
  }

}
