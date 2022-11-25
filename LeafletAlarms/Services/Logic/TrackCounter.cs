using Domain.StateWebSock;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LeafletAlarms.Services.Logic
{
  public class TrackCounter
  {
    private HashSet<string> _objectsInZone = new HashSet<string>();
    private string _logic_id;
    public TrackCounter(string logic_id)
    {
      _logic_id = logic_id;
    }

    public void InitZone(List<string> objs)
    {
      _objectsInZone = objs.ToHashSet();
    }
    public bool Process(List<TrackPointDTO> listOutZone, List<TrackPointDTO> listInZone)
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

    public List<string> GetInZones()
    {
      return _objectsInZone.ToList();
    }

    public int GetInZonesCount()
    {
      return _objectsInZone.Count;
    }
  }
}
