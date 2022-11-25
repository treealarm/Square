using Domain.StateWebSock;
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

    public void Found(List<TrackPointDTO> listOfTracks)
    {
      foreach (TrackPointDTO trackPoint in listOfTracks)
      {
        if (!_objectsInZone.Contains(trackPoint.figure.id))
        {
          _objectsInZone.Add(trackPoint.figure.id);
        }
      }
    }

    public void NotFound(List<TrackPointDTO> listOfTracks)
    {
      foreach (TrackPointDTO trackPoint in listOfTracks)
      {
        if (_objectsInZone.Contains(trackPoint.figure.id))
        {
          _objectsInZone.Remove(trackPoint.figure.id);
        }
      }
    }

    public List<string> GetInZones()
    {
      return _objectsInZone.ToList();
    }
  }
}
