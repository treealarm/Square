using DbLayer;
using Domain;
using Domain.StateWebSock;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver.GeoJsonObjectModel;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace LeafletAlarms.Controllers
{  
  public class StateWebSocket
  {
    private HttpContext _context;
    private WebSocket _webSocket;
    private MapService _mapService;
    private System.Timers.Timer _timer;
    private HashSet<string> _dicIds = new HashSet<string>();
    private object _locker = new object();
    private BoxDTO _currentBox;
    public BoxDTO CurrentBox
    {
      get
      {
        lock(_locker)
        {
          return _currentBox;
        }        
      }
    }
    void InitTimer()
    {
      _timer = new System.Timers.Timer();
      _timer.Interval = 1000;
      _timer.AutoReset = false;
      _timer.Elapsed += new ElapsedEventHandler(OnElapsed);
      _timer.Enabled = true;
    }

    private async void OnElapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
      // let timer start ticking
      _timer.Enabled = true;

      List<MarkerVisualState> stateList = new List<MarkerVisualState>();

      StateBaseDTO packet = new StateBaseDTO()
      {
        action = "set_visual_states",
        data = stateList
      };      

      lock (_locker)
      {
        if (_dicIds.Count == 0)
        {
          return;
        }

        int i = 1;
        Random rnd = new Random();

        foreach (var id in _dicIds)
        {
          i++;
          

          MarkerVisualState state = new MarkerVisualState()
          {
            id = id,
            color = System.Drawing.ColorTranslator.ToHtml(
              System.Drawing.Color.FromArgb(255, rnd.Next(1, 254), rnd.Next(1, 254), rnd.Next(1, 254)))
          };

          stateList.Add(state);
        }
      }

      await SendPacket(packet);            
    }

    public StateWebSocket(
      HttpContext context,
      WebSocket webSocket,
      MapService mapsService
    )
    {
      _mapService = mapsService;
      _context = context;
      _webSocket = webSocket;

      InitTimer();
    }

    public async Task ProcessAcceptedSocket()
    {
      var buffer = new byte[1024 * 4];
      WebSocketReceiveResult result = 
        await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

      while (!result.CloseStatus.HasValue)
      {
        string s = System.Text.Encoding.UTF8.GetString(buffer, 0, result.Count);

        StateBaseDTO json = JsonSerializer.Deserialize<StateBaseDTO>(s);

        if (json.action.ToString() == "set_box")
        {
          BoxDTO setBox = JsonSerializer.Deserialize<BoxDTO>(json.data.ToString()); ;

          if (setBox != null)
          {
            await OnSetBox(setBox);
          }
        }
        var replay = JsonSerializer.SerializeToUtf8Bytes(json);

        await _webSocket.SendAsync(
          new ArraySegment<byte>(buffer, 0, replay.Length),
          result.MessageType,
          result.EndOfMessage,
          CancellationToken.None
        );

        result = await _webSocket.ReceiveAsync(
          new ArraySegment<byte>(buffer),
          CancellationToken.None
        );
      }

      _timer.Enabled = false;

      await _webSocket.CloseAsync(
        result.CloseStatus.Value,
        result.CloseStatusDescription,
        CancellationToken.None
      );
    }

    public bool IsWithinBox(BoxDTO box, DBGeoObject track, List<string> levels)
    {
      bool IsPointInBox(BoxDTO box, GeoJson2DCoordinates coord, double dx = 0)
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

      if (track.location is GeoJsonPoint<GeoJson2DCoordinates> point)
      {
        var dx = track.radius / 111000;
        return IsPointInBox(box, point.Coordinates, dx);
      }

      if (track.location is GeoJsonPolygon<GeoJson2DCoordinates> polygon)
      {
        foreach (var pt in polygon.Coordinates.Exterior.Positions)
        {
          if (IsPointInBox(box, pt))
          {
            return true;
          }
        }
      }

      if (track.location is GeoJsonLineString<GeoJson2DCoordinates> line)
      {
        foreach (var pt in line.Coordinates.Positions)
        {
          if (IsPointInBox(box, pt))
          {
            return true;
          }
        }
      }

      return false;
    }

    public async Task OnUpdatePosition(List<DBTrackPoint> movedMarkers)
    {
      HashSet<string> toUpdate = new HashSet<string>();
      BoxDTO curBox = CurrentBox;

      if (curBox == null)
      {
        return;
      }

      List<DBTrackPoint> toCheckIfInBox = new List<DBTrackPoint>();
      var levels = await _mapService.LevelServ.GetLevelsByZoom(curBox.zoom);

      lock (_locker)
      {
        foreach (var track in movedMarkers)
        {
          if (_dicIds.Contains(track.figure.id))
          {
            if (!IsWithinBox(curBox, track.figure, levels))
            {
              _dicIds.Remove(track.figure.id);
            }
            toUpdate.Add(track.figure.id);
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
          _dicIds.Add(track.figure.id);
        }
      }

      if (toUpdate.Count > 0)
      {
        StateBaseDTO packet = new StateBaseDTO()
        {
          action = "set_ids2update",
          data = toUpdate
        };

        await SendPacket(packet);
      }
    }

    private async Task SendPacket(StateBaseDTO packet)
    {
      var buffer = JsonSerializer.SerializeToUtf8Bytes(packet);

      await _webSocket.SendAsync(
        new ArraySegment<byte>(buffer, 0, buffer.Length),
        WebSocketMessageType.Text,
        true,
        CancellationToken.None
      );
    }

    private async Task OnSetBox(BoxDTO box)
    {
      var geo = await _mapService.GeoServ.GetGeoAsync(box);

      lock(_locker)
      {
        _currentBox = box;

        _dicIds.Clear();

        foreach (var item in geo)
        {
          _dicIds.Add(item.id);
        }
      }
    }
  }
}
