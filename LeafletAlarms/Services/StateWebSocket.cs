using Domain;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace LeafletAlarms.Services
{
  public class StateWebSocket
  {
    private HttpContext _context;
    private WebSocket _webSocket;

    private IStateService _stateService;
    private IGeoService _geoService;
    private IMapService _mapService;
    private IValuesService _valuesService;

    private Task _worker;
    private CancellationTokenSource _cancellationTokenSource
      = new CancellationTokenSource();

    private HashSet<string> _dicIds = new HashSet<string>();
    
    private Dictionary<string, GeoObjectDTO> _dicGeo = new Dictionary<string, GeoObjectDTO>();
    private Dictionary<string, ObjectStateDTO> _dicStates = new Dictionary<string, ObjectStateDTO>();
    private Dictionary<string, AlarmState> _dicAlarmStates = new Dictionary<string, AlarmState>();
    private Dictionary<string, ValueDTO> _dicValues = new Dictionary<string, ValueDTO>();

    private Dictionary<string, BaseMarkerDTO> _dicOwnersAndViews = new Dictionary<string, BaseMarkerDTO>();
    private object _locker = new object();
    private BoxDTO _currentBox;
    private bool _update_values_periodically = false;
    private ConcurrentQueue<StateBaseReceiveDTO> _queue = new ConcurrentQueue<StateBaseReceiveDTO>();
    private readonly IServiceProvider _serviceProvider;

    public StateWebSocket(
      IServiceProvider serviceProvider
    )
    {
      _serviceProvider = serviceProvider;
    }

    public void Init(HttpContext context, WebSocket webSocket)
    {
      _context = context;
      _webSocket = webSocket;

      _worker = new Task(() => DoWork(), _cancellationTokenSource.Token);
      _worker.Start();
    }

    private async void DoWork()
    {
      while (!_cancellationTokenSource.IsCancellationRequested)
      {
        try
        {
          using var scope = _serviceProvider.CreateScope();
          _stateService = scope.ServiceProvider.GetRequiredService<IStateService>();
          _geoService = scope.ServiceProvider.GetRequiredService<IGeoService>();
          _mapService = scope.ServiceProvider.GetRequiredService<IMapService>();
          _valuesService = scope.ServiceProvider.GetRequiredService<IValuesService>();

          await Task.Delay(1000);

          while (_queue.TryDequeue(out var json))
          {
            await ProcessBuffer(json);
          }

          await PollBox();
          await PollStates();
          await PollValues();
        }
        catch(Exception ex)
        {
          Console.WriteLine(ex.ToString());
        }
      }
    }
    private async Task ProcessBuffer(StateBaseReceiveDTO json)
    {
      switch (json.action.ToString())
      {
        case "set_box":
          BoxDTO setBox = JsonSerializer.Deserialize<BoxDTO>(json.data.ToString());
          if (setBox != null)
            await OnSetBox(setBox);
          break;

        case "set_ids":
          List<string> ids = JsonSerializer.Deserialize<List<string>>(json.data.ToString());
          if (ids != null)
            await OnSetIds(ids);
          break;

        case "update_values_periodically":
          _update_values_periodically = JsonSerializer.Deserialize<bool>(json.data.ToString());
          break;
      }
    }


    public async Task ProcessAcceptedSocket()
    {
      var buffer = new byte[1024 * 4];
      WebSocketReceiveResult result;

      try
      {
        while (_webSocket.State == WebSocketState.Open)
        {
          result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

          if (result.CloseStatus.HasValue)
            break;

          string s = Encoding.UTF8.GetString(buffer, 0, result.Count);
          var json = JsonSerializer.Deserialize<StateBaseReceiveDTO>(s);

          if (json != null)
          {
            _queue.Enqueue(json); // кладём в очередь на обработку
          }
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex);
      }
      finally
      {
        _cancellationTokenSource.Cancel();
      }
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

    private async Task UpdateIds(HashSet<string> toUpdate)
    {
      if (toUpdate.Count == 0)
      {
        return;
      }
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
    private async Task DeleteIds(HashSet<string> toDelete)
    {
      if (toDelete.Count == 0)
      {
        return;
      }
      StateBaseDTO packet = new StateBaseDTO()
      {
        action = "set_ids2delete",
        data = toDelete
      };

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
    
    private async Task OnStateChanged(List<string> ids)
    {
      if (ids.Count == 0)
      { return; }

      HashSet<string> objIds = GetOwnersAndViews(ids);

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

    public async Task OnValuesChanged(List<string> ids)
    {
      HashSet<string> objIds = GetOwnersAndViews(ids);

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

    private async Task PollValues()
    {
      if (!_update_values_periodically)
        return;

      if (_dicIds.Count == 0)
      {
        _dicValues.Clear();
        return;
      }

      // 1. Получаем актуальные значения
      var list = await _valuesService.GetListByOwnersAsync(_dicIds.ToList());


      // 3. Определяем изменения
      var changedIds = new HashSet<string>();


      // added / updated
      foreach (var value in list)
      {
        if (!_dicValues.TryGetValue(value.Key, out var old) ||
            old.Version != value.Value.Version)
        {
          changedIds.Add(value.Value.owner_id);
        }
      }

      // 4. Обновляем кэш
      _dicValues = list;

      // 5. Уведомляем
      if (changedIds.Count > 0)
      {
        await OnValuesChanged(changedIds.ToList());
      }
    }



    private async Task PollStates()
    {
      if (_dicIds.Count == 0)
      {
        _dicStates.Clear();
        _dicAlarmStates.Clear();
        return;
      }

      var changedIds = new HashSet<string>();

      // ---------- обычные состояния ----------
      {
        var states = await _stateService.GetStatesAsync(_dicIds.ToList());

        // removed
        foreach (var id in _dicStates.Keys.Except(states.Keys))
          changedIds.Add(id);

        // added / updated
        foreach (var (id, state) in states)
        {
          if (!_dicStates.TryGetValue(id, out var old) ||
              old.Version != state.Version)
          {
            changedIds.Add(id);
          }
        }

        _dicStates = states;
      }

      // ---------- alarm-состояния ----------
      {
        var alarmStates = await _stateService.GetAlarmStatesAsync(_dicIds.ToList());

        // removed
        foreach (var id in _dicAlarmStates.Keys.Except(alarmStates.Keys))
          changedIds.Add(id);

        // added / updated
        foreach (var (id, state) in alarmStates)
        {
          if (!_dicAlarmStates.TryGetValue(id, out var old) ||
              old.Version != state.Version)
          {
            changedIds.Add(id);
          }
        }

        _dicAlarmStates = alarmStates;
      }

      // ---------- единое уведомление ----------
      if (changedIds.Count > 0)
      {
        await OnStateChanged(changedIds.ToList());
      }
    }


    private async Task PollBox()
    {
      BoxDTO box;
      box = _currentBox;

      if (box == null)
      {
         _dicGeo.Clear();
         return;
      }
        

      // 1. Получаем актуальные геообъекты для box
      var geo = await _geoService.GetGeoAsync(box);

      // 2. Сравниваем с текущим кэшем _dicGeo
      var added = new Dictionary<string, GeoObjectDTO>();
      var updated = new Dictionary<string, GeoObjectDTO>();
      var removed = new HashSet<string>();

      // 2a. removed — есть в _dicGeo, но нет в geo
      removed = _dicGeo.Keys.Except(geo.Keys).ToHashSet();

      // 2b. added / updated
      foreach (var (id, obj) in geo)
      {
        if (!_dicGeo.TryGetValue(id, out var old))
        {
          added[id] = obj;
        }
        else if (old.Version != obj.Version)
        {
          updated[id] = obj;
        }
      }

      _dicGeo = geo;
      // 2c. обновляем _dicIds
      _dicIds.Clear();
      foreach (var id in _dicGeo.Keys)
        _dicIds.Add(id);


      // 3. Вызываем UpdateOwners только если есть изменения
      if (added.Any() || updated.Any() || removed.Any())
        await UpdateOwners();

      // 4. По желанию: можно сразу пушить изменения клиенту
      if (added.Any() || updated.Any() || removed.Any())
      {
        await UpdateIds(added.Keys.Union(updated.Keys).ToHashSet());
        await DeleteIds(removed);
      }      
    }

    private async Task OnSetBox(BoxDTO box)
    {
      lock (_locker)
      {
        _currentBox = box;
      }
      await Task.CompletedTask;
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
      }

      await UpdateOwners();
    }
  }
}
