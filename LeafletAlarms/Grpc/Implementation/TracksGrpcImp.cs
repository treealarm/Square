using Domain;
using Domain.GeoDBDTO;
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
    public TracksGrpcImp(
      ILogger<TracksGrpcImp> logger,
      TracksUpdateService trackUpdateService)
    {
      _logger = logger;
      _trackUpdateService = trackUpdateService;
    }


    public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
    {
      Console.WriteLine($"Received from:{request.Name}");
      return Task.FromResult(new HelloReply
      {
        Message = "Hello " + request.Name + $"[{DateTime.Now}]"
      });
    }

    public override async Task<ProtoFigures> AddTracks(ProtoFigures request, ServerCallContext context)
    {
      ProtoFigures response = new ProtoFigures();

      Console.WriteLine($"Received from:{request.ToString()}");
      var figs = new FiguresDTO();
      figs.figs = new List<FigureGeoDTO>();

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

      var result = await _trackUpdateService.AddTracks(figs);

      foreach (var resFig in result.figs)
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
  }
}
