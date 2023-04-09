using DbLayer.Services;
using Domain;
using Domain.GeoDBDTO;
using Domain.ServiceInterfaces;
using Domain.States;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using LeafletAlarms.Services;
using LeafletAlarmsGrpc;
using System.Text.Json;
using static LeafletAlarmsGrpc.TracksGrpcService;

namespace LeafletAlarms.Grpc.Implementation
{
  public class TracksGrpcImp: TracksGrpcServiceBase
  {
    private readonly ILogger<TracksGrpcImp> _logger;
    private readonly TracksUpdateService _trackUpdateService;
    private readonly StatesUpdateService _statesUpdateService;
    public TracksGrpcImp(
      ILogger<TracksGrpcImp> logger,
      TracksUpdateService trackUpdateService,
      StatesUpdateService statesUpdateService
    )
    {
      _logger = logger;
      _trackUpdateService = trackUpdateService;
      _statesUpdateService = statesUpdateService;
    }

    public override async Task<ProtoFigures> UpdateFigures(ProtoFigures request, ServerCallContext context)
    {
      ProtoFigures response = new ProtoFigures();

      Console.WriteLine($"Received from:{request.ToString()}");
      var figs = new FiguresDTO();
      figs.figs = new List<FigureGeoDTO>();
      figs.add_tracks = request.AddTracks;

      foreach (var fig in request.Figs)
      {
        var newFigDto = new FigureGeoDTO();
        newFigDto.id = fig.Id;
        newFigDto.name = fig.Name;
        newFigDto.external_type = fig.ExternalType;
        newFigDto.parent_id = fig.ParentId;
        newFigDto.radius = fig.Radius;
        newFigDto.zoom_level = fig.ZoomLevel;

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

        if (fig.Geometry.Type == "Polygon")
        {          
          var polygonCoord = new GeometryPolygonDTO();
          newFigDto.geometry = polygonCoord;
          polygonCoord.coord = new List<Geo2DCoordDTO>();

          foreach (var c in fig.Geometry.Coord)
          {
            polygonCoord.coord.Add(new Geo2DCoordDTO()
            {
              Lon = c.Lon,
              Lat = c.Lat
            });
          }
        }        
      }

      await _trackUpdateService.UpdateFigures(figs);

      foreach (var resFig in figs.figs)
      {
        var pFig = new ProtoFig()
        {
          Id = resFig.id,
          ParentId = resFig.parent_id,
          ExternalType = resFig.external_type,
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

    public override async Task<BoolValue> UpdateStates(ProtoObjectStates request, ServerCallContext context)
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
      ret.Value =  await _statesUpdateService.UpdateStates(objStates);

      return ret;
    }
  }
}
