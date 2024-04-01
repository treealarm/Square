using Domain.GeoDBDTO;
using Domain.GeoDTO;
using Domain.States;
using Domain.StateWebSock;
using Domain;
using Google.Protobuf.WellKnownTypes;
using LeafletAlarms.Services;
using LeafletAlarmsGrpc;
using Domain.ServiceInterfaces;
using Microsoft.Extensions.Logging;
using Domain.Events;

namespace LeafletAlarms.Grpc.Implementation
{
  public class GRPCServiceProxy
  {
    private readonly ILogger<GRPCServiceProxy> _logger;
    private readonly TracksUpdateService _trackUpdateService;
    private readonly StatesUpdateService _statesUpdateService;
    private readonly IEventsService _eventsService;
    public GRPCServiceProxy(
      ILogger<GRPCServiceProxy> logger,
      TracksUpdateService trackUpdateService,
      StatesUpdateService statesUpdateService,
      IEventsService eventsService
    )
    {
      _logger = logger;
      _trackUpdateService = trackUpdateService;
      _statesUpdateService = statesUpdateService;
      _eventsService = eventsService;
    }
    private GeometryDTO CoordsFromProto2DTO(ProtoGeometry geometry)
    {
      var geo = new GeometryDTO();

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

    public async Task<ProtoFigures> UpdateFigures(ProtoFigures request)
    {
      ProtoFigures response = new ProtoFigures();

      //Console.WriteLine($"Received from:{request.ToString()}");
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

        newFigDto.geometry = CoordsFromProto2DTO(fig.Geometry);
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

    public async Task<BoolValue> UpdateEvents(EventsProto request)
    {
      var list = new List<EventDTO>();

      foreach(var ev in request.Events)
      {
        var pEvent = new EventDTO();

        pEvent.timestamp = ev.Timestamp.ToDateTime();
        pEvent.meta = new EventMetaDTO();

        pEvent.meta.id = ev.Meta.Id;
        pEvent.meta.object_id = ev.Meta.ObjectId;
        pEvent.meta.event_name = ev.Meta.EventName;
        pEvent.meta.event_priority = ev.Meta.EventPriority;
        pEvent.extra_props = new List<ObjExtraPropertyDTO>();

        foreach (var e in ev.ExtraProps)
        {
          pEvent.extra_props.Add(new ObjExtraPropertyDTO()
          {
            prop_name = e.PropName,
            str_val = e.StrVal,
            visual_type = e.VisualType
          });
        }
        list.Add(pEvent);
      }
      var count = await _eventsService.InsertManyAsync(list);
      return new BoolValue() { Value = count == list.Count};
    }
  }
}
