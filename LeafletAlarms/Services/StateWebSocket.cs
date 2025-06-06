﻿using Domain;
using System.Net.WebSockets;
using System.Text.Json;

namespace LeafletAlarms.Services
{
  public class StateWebSocket
  {
    private HttpContext _context;
    private WebSocket _webSocket;
    private IStateService _stateService;
    private IGeoService _geoService;
    private ILevelService _levelService;
    private IMapService _mapService;
    private IPubService _pub;

    private Task _timer;
    private CancellationTokenSource _cancellationTokenSource
      = new CancellationTokenSource();

    Dictionary<string, TrackPointDTO> _dicUpdatedTracks =
      new Dictionary<string, TrackPointDTO>();
    private object _lockerTracks = new object();

    private HashSet<string> _setTrackUpdate = new HashSet<string>();
    private HashSet<string> _setIdsToUpdate = new HashSet<string>();
    private HashSet<string> _dicIds = new HashSet<string>();
    private Dictionary<string, BaseMarkerDTO> _dicOwnersAndViews = new Dictionary<string, BaseMarkerDTO>();
    private object _locker = new object();
    private BoxDTO _currentBox;
    private bool _update_values_periodically = false;
    public BoxDTO CurrentBox
    {
      get
      {
        lock (_locker)
        {
          return _currentBox;
        }
      }
    }

    public StateWebSocket(
      HttpContext context,
      WebSocket webSocket,
      IGeoService geoService,
      ILevelService levelService,
      IStateService stateService,
      IMapService mapService,
      IPubService pub
    )
    {
      _stateService = stateService;
      _geoService = geoService;
      _context = context;
      _webSocket = webSocket;
      _levelService = levelService;
      _mapService = mapService;
      _pub = pub;
      InitTimer();
    }

    void InitTimer()
    {
      _timer = new Task(() => OnElapsed(), _cancellationTokenSource.Token);
      _timer.Start();
    }

    private async void OnElapsed()
    {
      while (!_cancellationTokenSource.IsCancellationRequested)
      {
        try
        {
          await Task.Delay(1000);

          List<TrackPointDTO> movedMarkers;
          HashSet<string> idsToUpdate;
          HashSet<string> setTrackUpdate;

          lock (_lockerTracks)
          {
            movedMarkers = _dicUpdatedTracks.Values.ToList();
            _dicUpdatedTracks.Clear();
          }

          lock (_locker)
          {
            idsToUpdate = _setIdsToUpdate.ToHashSet();
            _setIdsToUpdate.Clear();

            setTrackUpdate = _setTrackUpdate.ToHashSet();
            _setTrackUpdate.Clear();
          }

          if (idsToUpdate.Any())
          {
            await UpdateIds(idsToUpdate);
          }

          if (movedMarkers.Any())
          {
            await DoUpdateTrackPosition(movedMarkers);
          }

          if (setTrackUpdate.Any())
          {
            await UpdateRoutesByTrackId(setTrackUpdate);
          }          
        }
        catch(Exception ex)
        {
          Console.WriteLine(ex.ToString());
        }
      }
    }

    public async Task ProcessAcceptedSocket()
    {
      var buffer = new byte[1024 * 4];
      WebSocketReceiveResult result =
        await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

      while (!result.CloseStatus.HasValue)
      {
        string s = System.Text.Encoding.UTF8.GetString(buffer, 0, result.Count);

        var json = JsonSerializer.Deserialize<StateBaseReceiveDTO>(s);

        if (json.action.ToString() == "set_box")
        {
          BoxDTO setBox = JsonSerializer.Deserialize<BoxDTO>(json.data.ToString());

          if (setBox != null)
          {
            await OnSetBox(setBox);
          }
        }

        if (json.action.ToString() == "set_ids")
        {
          List<string> ids = JsonSerializer.Deserialize<List<string>>(json.data.ToString());

          if (ids != null)
          {
            await OnSetIds(ids);
          }
        }

        if (json.action.ToString() == "update_values_periodically")
        {
          _update_values_periodically = JsonSerializer.Deserialize<bool>(json.data.ToString());
        }

        var replay = JsonSerializer.SerializeToUtf8Bytes(json);

        //await _webSocket.SendAsync(
        //  new ArraySegment<byte>(buffer, 0, replay.Length),
        //  result.MessageType,
        //  result.EndOfMessage,
        //  CancellationToken.None
        //);

        result = await _webSocket.ReceiveAsync(
          new ArraySegment<byte>(buffer),
          CancellationToken.None
        );
      }

      _cancellationTokenSource.Cancel();
      _timer.Wait();

      await _webSocket.CloseAsync(
        result.CloseStatus.Value,
        result.CloseStatusDescription,
        CancellationToken.None
      );
    }

    public bool IsWithinBox(BoxDTO box, GeoObjectDTO track, List<string> levels)
    {
      bool IsPointInBox(BoxDTO box, Geo2DCoordDTO coord, double dx = 0)
      {
        var right = box.es[0] + dx;
        var left = box.wn[0] - dx;
        var up = box.wn[1] + dx;
        var down = box.es[1] - dx;

        if (coord.X < left)
          return false;

        if (coord.X > right)
          return false;

        if (coord.Y > up)
          return false;

        if (coord.Y < down)
          return false;

        return true;
      }

      if (!string.IsNullOrEmpty(track.zoom_level))
      {
        if (!levels.Contains(track.zoom_level))
        {
          return false;
        }
      }

      if (track.location is GeometryCircleDTO point)
      {
        double dx = (double)(track.radius == null ? 0.0 : track.radius / 111000);
        return IsPointInBox(box, point.coord, dx);
      }

      if (track.location is GeometryPolygonDTO polygon)
      {
        foreach (var pt in polygon.coord)
        {
          if (IsPointInBox(box, pt))
          {
            return true;
          }
        }
      }

      if (track.location is GeometryPolylineDTO line)
      {
        foreach (var pt in line.coord)
        {
          if (IsPointInBox(box, pt))
          {
            return true;
          }
        }
      }

      return false;
    }

    public int GetCurObjectCount()
    {
      lock (_locker)
      {
        return _dicIds.Count;
      }
    }

    public void OnUpdateTrackPosition(List<TrackPointDTO> movedMarkers)
    {
      lock(_lockerTracks)
      {
        foreach(var track in movedMarkers)
        {
          if (string.IsNullOrEmpty(track.figure.id))
          {
            continue;
          }
          _dicUpdatedTracks[track.figure.id] = track;
        }
      }      
    }

    public void OnUpdateTracks(List<string> routEnds)
    {
      lock (_locker)
      {
        foreach (var track_end_id in routEnds)
        {
          _setTrackUpdate.Add(track_end_id);
        }
      }
    }    

    private async Task UpdateIds(HashSet<string> toUpdate)
    {
      StateBaseDTO packet = new StateBaseDTO()
      {
        action = "set_ids2update",
        data = toUpdate
      };

      if (toUpdate.Count > 1000 || GetCurObjectCount() >  1000)
      {
        packet.action = "update_viewbox";
        packet.data = null;
      }

      await SendPacket(packet);
    }

    private async Task UpdateRoutesByTrackId(HashSet<string> toUpdate)
    {
      StateBaseDTO packet = new StateBaseDTO()
      {
        action = "update_routes_by_tracks",
        data = toUpdate
      };

      await SendPacket(packet);
    }

    private async Task DoUpdateTrackPosition(List<TrackPointDTO> movedMarkers)
    {
      HashSet<string> toUpdate = new HashSet<string>();
      HashSet<string> toDelete = new HashSet<string>();
      BoxDTO curBox = CurrentBox;

      if (curBox == null)
      {
        return;
      }

      var toCheckIfInBox = new List<TrackPointDTO>();
      var levels = await _levelService.GetLevelsByZoom(curBox.zoom);

      lock (_locker)
      {
        foreach (var track in movedMarkers)
        {
          if (_dicIds.Contains(track.figure.id))
          {
            if (!IsWithinBox(curBox, track.figure, levels))
            {
              _dicIds.Remove(track.figure.id);
              toDelete.Add(track.figure.id);
            }
            else
            {
              toUpdate.Add(track.figure.id);
            }
            
            continue;
          }
          toCheckIfInBox.Add(track);
        }
      }

      foreach (var track in toCheckIfInBox)
      {
        if (IsWithinBox(curBox, track.figure, levels))
        {
          toUpdate.Add(track.figure.id);
          lock (_locker)
          {
            _dicIds.Add(track.figure.id);
          }
        }
      }
      await UpdateOwners();
      await _pub.Publish(Topics.CheckStatesByIds, toUpdate);

      if (toDelete.Count > 0)
      {
        StateBaseDTO packet = new StateBaseDTO()
        {
          action = "set_ids2delete",
          data = toDelete
        };

        await SendPacket(packet);
      }
      
      if (toUpdate.Count > 0)
      {
        await RemoveFilteredIds(curBox, toUpdate);

        await UpdateIds(toUpdate);
      }
    }

    public async Task RemoveFilteredIds(BoxDTO curBox, HashSet<string> toUpdate)
    {
      if (curBox.property_filter != null && curBox.property_filter.props.Count > 0)
      {
        var toCompare = curBox.property_filter.props;

        var props = await _mapService.GetPropsAsync(toUpdate.ToList());

        foreach (var prop in props)
        {
          foreach (var c in toCompare)
          {
            var objProp = prop.Value.extra_props
              .Where(i => i.prop_name == c.prop_name)
              .FirstOrDefault();

            if (objProp == null || objProp.str_val != c.str_val)
            {
              toUpdate.Remove(prop.Key);
              break;
            }
          }
        }
      }
    }

    public HashSet<string> GetOwnersAndViews(IEnumerable<string> ids)
    {
      HashSet<string> objIds = new HashSet<string>();

      lock (_locker)
      {
        foreach (var id in ids)
        {
          if (_dicOwnersAndViews.TryGetValue(id, out var marker))
          {
            var views = _dicOwnersAndViews.Values
              .Where(i => i.owner_id == marker.id)
              .Select(i => i.id)
              ;
            objIds.UnionWith(views);
            objIds.Add(id);
          }
        }
      }
      return objIds;
    }
    
    public async Task OnStateChanged(List<ObjectStateDTO> states)
    {
      HashSet<string> objIds = GetOwnersAndViews(states.Select(i=>i.id));

      if (objIds.Count > 0)
      {
        StateBaseDTO packet = new StateBaseDTO()
        {
          action = "set_visual_states",
          data = objIds.ToList()
        };
        await SendPacket(packet);
      }
    }

    public async Task OnValuesChanged(List<ValueDTO> states)
    {
      if (!_update_values_periodically)
      {
        return;
      }

      HashSet<string> objIds = GetOwnersAndViews(states.Select(i => i.owner_id));

      if (!objIds.Any())
      {
        return;
      }

      StateBaseDTO packet = new StateBaseDTO()
      {
        action = "on_values_changed",
        data = objIds
      };

      await SendPacket(packet);
    }

    public async Task OnBlinkStateChanged(List<AlarmState> states)
    {
      HashSet<string> objIds = GetOwnersAndViews(states.Select(i => i.id));

      StateBaseDTO packet = new StateBaseDTO()
      {
        action = "set_alarm_states",
        data = objIds
      };

      await SendPacket(packet);
    }

    private async Task SendPacket(StateBaseDTO packet)
    {
      var buffer = JsonSerializer.SerializeToUtf8Bytes(packet);

      await _webSocket.SendAsync(
        new ArraySegment<byte>(buffer, 0, buffer.Length),
        WebSocketMessageType.Text,
        true,
        _cancellationTokenSource.Token
      );
    }

    private async Task OnSetBox(BoxDTO box)
    {
      var geo = await _geoService.GetGeoAsync(box);

      lock (_locker)
      {
        _currentBox = box;

        var newIds = geo.Where(g => !_dicIds.Contains(g.Key));

        _dicIds.Clear();


        foreach (var item in geo)
        {
          _dicIds.Add(item.Key);
        }

        _pub.Publish(Topics.CheckStatesByIds, newIds.Select(i => i.Key).ToList());
      }
    }

    private async Task UpdateOwners()
    {
      List<string> ids;
      lock (_locker)
      {
        ids = _dicIds.ToList();
      }
      var owners_and_views = await _mapService.GetOwnersAndViewsAsync(ids);
      lock (_locker)
      {
        _dicOwnersAndViews = owners_and_views;
      }
    }
    private async Task OnSetIds(List<string> ids)
    {
      lock (_locker)
      {
        _currentBox = null;

        var newIds = ids.Where(g => !_dicIds.Contains(g));

        _dicIds.Clear();


        foreach (var item in ids)
        {
          _dicIds.Add(item);
        }
        _pub.Publish(Topics.CheckStatesByIds, newIds.ToList());
      }

      await UpdateOwners();
    }
  }
}
