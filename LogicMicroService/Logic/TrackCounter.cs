using DbLayer.Services;
using Domain;
using Domain.GeoDBDTO;
using Domain.GeoDTO;
using Domain.ServiceInterfaces;
using Domain.StateWebSock;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LogicMicroService
{
  public class TrackCounter : BaseLogicProc
  {
    private HashSet<string> _objectsInZone = new HashSet<string>();
    public List<GeometryDTO> Zone { get; set; }

    public TrackCounter(StaticLogicDTO logicDto)
      :base(logicDto)
    {
    }

    public override async Task InitFromDb(IGeoService geoService, IMapService mapService)
    {
      await base.InitFromDb(geoService, mapService);

      var logicObjs = LogicDTO.figs.Where(f => f.group_id != "gr_text").ToList();

      var geoFigs = await
          geoService.GetGeoObjectsAsync(logicObjs.Select(f => f.id).ToList());

      var zones = geoFigs.Values.ToList();

      BoxTrackDTO box = new BoxTrackDTO();
      box.ids = PropFilterObjIds.ToList();
      box.property_filter = LogicDTO.property_filter;

      Zone = zones.Select(f => f.location).ToList();
      box.zone = Zone; 
      var objectsNowInZone = await geoService.GetGeoAsync(box);

      var removeFigs = LogicDTO.figs.Select(f => f.id).ToHashSet();

      if (objectsNowInZone != null)
      {
        _objectsInZone = objectsNowInZone.Values
          .Select(f => f.id)
          .Where(d => !removeFigs.Contains(d))
          .ToHashSet();
      }      
    }

    private bool Process(List<TrackPointDTO> listOutZone, List<TrackPointDTO> listInZone)
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
          if (!_objectsInZone.Contains(id))
          {
            bChanged = true;
            _objectsInZone.Add(id);
          }          
        }
        else
        {
          if (_objectsInZone.Contains(id))
          {
            bChanged = true;
            _objectsInZone.Remove(id);
          }            
        }
      }

      return bChanged;
    }

    private List<string> GetInZones()
    {
      return _objectsInZone.ToList();
    }

    public override string GetUpdatedResult()
    {
      return _objectsInZone.Count.ToString();
    }

    public override async Task<bool> ProcessTracks(
      ITrackService tracksService,
      DateTime? time_start,
      DateTime? time_end
      )
    {
      BoxTrackDTO box = new BoxTrackDTO();
      box.time_start = time_start;
      box.time_end = time_end;
      box.zone = this.Zone;
      box.ids = PropFilterObjIds.ToList();
      box.property_filter = LogicDTO.property_filter;

      // Get what we have for current time;
      var inZones = this.GetInZones();

      box.ids = null;
      box.not_in_zone = false;
      var tracksInZone = await tracksService.GetTracksByBox(box);

      // Add figures which were in zone for period in case they cross border.
      var listInZone = tracksInZone.Select(t => t.figure.id).ToList();
      inZones.AddRange(listInZone);

      List<TrackPointDTO> tracksOutZone = null;

      if (inZones.Count > 0)
      {
        box.not_in_zone = true;
        box.ids = inZones.Distinct().ToList();
        tracksOutZone = await tracksService.GetTracksByBox(box);
      }

      bool bChanged = this.Process(tracksOutZone, tracksInZone);

      return bChanged;
    }
  }
}
