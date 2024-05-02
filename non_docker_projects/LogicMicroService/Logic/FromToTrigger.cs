using DbLayer.Services;
using Domain;
using Domain.GeoDBDTO;
using Domain.GeoDTO;
using Domain.ServiceInterfaces;
using Domain.StateWebSock;
using System.Collections.Generic;

namespace LogicMicroService
{
  public class FromToTrigger: BaseLogicProc
  {
    private Dictionary<string, DateTime> _objectsInFromZone =
      new Dictionary<string, DateTime>();

    private int _count = 0;
    private DateTime _lastUpdate = DateTime.UtcNow;
    public List<GeometryDTO> FromZone { get; set; }
    public List<GeometryDTO> ToZone { get; set; }
    public FromToTrigger(StaticLogicDTO logicDto)
      :base(logicDto)
    {

    }

    public override async Task InitFromDb(IGeoService geoService, IMapService mapService)
    {
      await base.InitFromDb(geoService, mapService);

      var logicFromObjs = LogicDTO.figs.Where(f => f.group_id == "from").ToList();
      var logicToObjs = LogicDTO.figs.Where(f => f.group_id == "to").ToList();

      var geoFigsFrom = 
        await geoService.GetGeoObjectsAsync(logicFromObjs
        .Select(f => f.id)
        .ToList());

      var geoFigsTo =
        await geoService.GetGeoObjectsAsync(logicToObjs
        .Select(f => f.id)
        .ToList());

      var zonesFrom = geoFigsFrom.Values.ToList();
      var zonesTo = geoFigsTo.Values.ToList();

      var removeFigs = LogicDTO.figs.Select(f => f.id).ToHashSet();

      BoxTrackDTO box = new BoxTrackDTO();
      box.ids = PropFilterObjIds.ToList();
      box.property_filter = LogicDTO.property_filter;

      // To Zone.

      ToZone = zonesTo.Select(f => f.location).ToList();

      // From Zone.
      FromZone = zonesFrom.Select(f => f.location).ToList();
      box.zone = FromZone;

      var objectsNowInFromZone = await geoService.GetGeoAsync(box);

      _objectsInFromZone.Clear();

      if (objectsNowInFromZone != null)
      {        
        foreach (var obj in objectsNowInFromZone.Values)
        {
          if (removeFigs.Contains(obj.id))
          {
            continue;
          }
          _objectsInFromZone.Add(obj.id, DateTime.UtcNow);
        }
      }
    }

    public override string GetUpdatedResult()
    {
      return _count.ToString();
    }

    async Task<List<TrackPointDTO>> GetTracksEnteredZone(
      ITrackService tracksService,
      DateTime? time_start,
      DateTime? time_end,
      List<GeometryDTO> zone
    )
    {
      BoxTrackDTO box = new BoxTrackDTO();
      box.time_start = time_start;
      box.time_end = time_end;
      box.zone = zone;
      box.ids = PropFilterObjIds.ToList();
      box.property_filter = LogicDTO.property_filter;
      box.ids = null;
      box.not_in_zone = false;
      var tracksInZone = await tracksService.GetTracksByBox(box);

      return tracksInZone;
    }

    async Task<List<TrackPointDTO>> GetTracksLeftZone(
      ITrackService tracksService,
      DateTime? time_start,
      DateTime? time_end,
      List<GeometryDTO> zone,
      List<string> ids
    )
    {
      BoxTrackDTO box = new BoxTrackDTO();
      box.time_start = time_start;
      box.time_end = time_end;
      box.zone = zone;
      box.ids = PropFilterObjIds.ToList();
      box.property_filter = LogicDTO.property_filter;
      box.ids = ids;
      box.not_in_zone = true;
      var tracksInZone = await tracksService.GetTracksByBox(box);

      return tracksInZone;
    }

    public override async Task<bool> ProcessTracks(
      ITrackService tracksService,
      DateTime? time_start,
      DateTime? time_end
    )
    {
      var dt = DateTime.UtcNow;

      if ((dt - _lastUpdate).TotalMinutes > 30)
      {
        _lastUpdate = dt;
        _count = 0;
      }

      bool bChanged = false;
      var bFromZoneExist = FromZone != null && FromZone.Count > 0;
      var bToZoneExist = ToZone != null && ToZone.Count > 0;

      List<TrackPointDTO> objsEnteredFrom = null;
      List<TrackPointDTO> objsEnteredTo = null;

      if (bFromZoneExist)
      {
        objsEnteredFrom = await GetTracksEnteredZone(
          tracksService,
          time_start,
          time_end,
          FromZone
        );
      }

      if (bToZoneExist)
      {
        objsEnteredTo = await GetTracksEnteredZone(
          tracksService,
          time_start,
          time_end,
          ToZone
        );
      }

      if (bFromZoneExist && bToZoneExist)
      {
        bChanged = ProcessTracks(objsEnteredTo, objsEnteredFrom);
      }

      if (bFromZoneExist && !bToZoneExist)
      {
        objsEnteredTo = new List<TrackPointDTO>();
        ProcessTracks(objsEnteredTo, objsEnteredFrom);
        var ids = _objectsInFromZone.Keys.ToList();
        var leftFromZone = 
          await GetTracksLeftZone(tracksService, time_start, time_end, FromZone, ids);
        bChanged = ProcessTracks(leftFromZone, objsEnteredFrom);
      }

      if (!bFromZoneExist && bToZoneExist)
      {
        _count += objsEnteredTo.Count;
        bChanged = true;
      }

      var expired = new List<string>();
      
      var min_dt = dt;
      

      foreach (var obj in _objectsInFromZone)
      {
        if (obj.Value < min_dt)
        {
          min_dt = obj.Value;
        }

        if ((dt - obj.Value).TotalSeconds > 60)
        {
          expired.Add(obj.Key);
        }
      }

      if (expired.Count > 0)
      {
        var leftFromZone =
          await GetTracksLeftZone(tracksService, min_dt, time_end, FromZone, expired);

        foreach (var obj in leftFromZone)
        {
          _objectsInFromZone.Remove(obj.figure.id);
        }        
      }

      return bChanged;
    }

    private bool ProcessTracks(
      List<TrackPointDTO> listOutZone,
      List<TrackPointDTO> listInZone
    )
    {
      bool bChanged = false;

      List<TrackZonePosition> trackPoints = new List<TrackZonePosition>();

      if (listOutZone != null)
      {
        foreach (var trackPoint in listOutZone)
        {
          trackPoints.Add(
            new TrackZonePosition()
            {
              Track = trackPoint,
              IsInZone = false
            });
        }
      }

      if (listInZone != null)
      {
        foreach (var trackPoint in listInZone)
        {
          trackPoints.Add(
            new TrackZonePosition()
            {
              Track = trackPoint,
              IsInZone = true
            });
        }
      }

      var sorted = trackPoints.OrderBy(t => t.Track.timestamp);

      foreach (var trackPoint in sorted)
      {
        var id = trackPoint.Track.figure.id;

        if (trackPoint.IsInZone)
        {
          if (!_objectsInFromZone.ContainsKey(id))
          {
            _objectsInFromZone.Add(id, trackPoint.Track.timestamp);
          }
        }
        else
        {
          if (_objectsInFromZone.ContainsKey(id))
          {
            _count++;
            bChanged = true;
            _objectsInFromZone.Remove(id);
          }
        }
      }

      return bChanged;
    }

  }


}
