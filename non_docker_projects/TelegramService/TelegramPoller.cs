using Google.Protobuf.WellKnownTypes;
using GrpcDaprClientLib;
using LeafletAlarmsGrpc;
using System.Text.Json;

namespace TelegramService
{
  internal class TelegramPoller: IDisposable
  {
    private string _botId;
    private string _chatId;
    GrpcMover _client = new GrpcMover();
  

    private CancellationToken _cancellationToken = new CancellationToken();
    private bool _somethingChanged = false;

    private List<TelegramLocationRequest> _locationRequests = new List<TelegramLocationRequest>();
    private TelegramPoller() { }
    public TelegramPoller(string botId, string chatId)
    {
      _botId = botId;
      _chatId = chatId;
      _client.Connect($"http://localhost:5000");
    }

    private void ProcessCallback(TelegramService.Result r)
    {
      var btnData = r.callback_query.data.Split(':');

      if (btnData.Length < 2)
      {
        return;
      }

      if (btnData[0] == "a")
      {
        // Aknowleged button.
        if (long.TryParse(btnData[1], out var lVal))
        {
          _somethingChanged = true;
        }
      }
    }

    private async Task ProcessPhoto(
      TelegramService.Result r,
      string botId,
      string chat_id
    )
    {
      var photo = r.message.photo.LastOrDefault();

      if (photo == null)
      {
        return;
      }

      TelegramSender sender = new TelegramSender();

      var replay = await sender.GetPhoto(botId, photo.file_id);
      string fileNameRef = replay.fileNameRef;

      var path = $"{photo.file_unique_id}_{fileNameRef}";

      File.WriteAllBytes(path, replay.data);

      var replayLoc = await RequestLocation(botId, chat_id);

      if (replayLoc != null)
      {
        _locationRequests.Add(new TelegramLocationRequest()
        {
          ImageFile = Path.GetFileName(path),
          SourceMessage = replayLoc.result
        });
      }
    }

    private void ProcessReplay(Result r)
    {
      var replay_to_message = r.message.reply_to_message;
      var srcMessage = _locationRequests
        .Where(m => m.SourceMessage.message_id == replay_to_message.message_id)
        .FirstOrDefault();

      if (srcMessage != null)
      {
        _locationRequests.Remove(srcMessage);

      }
    }

    private async Task ProcessEditedMessage(Result r)
    {
      var edited_message = r.edited_message;
      
      if (edited_message?.location != null)
      {
        await GrpcSendPoint(edited_message);
      }
    }

    private async Task<TelegramSingleUpdate> RequestLocation(string botId, string chatId)
    {
      try
      {
        TelegramSender sender = new TelegramSender();

        var msg = new TelegramMessage()
        {
          bot_id = botId,
          chat_id = chatId,
          text = ("TelegramRequestLocation")
        };

        var reply_markup = new LocationMarkup()
        {
          keyboard = new List<List<KeyboardButton>>()
        };

        var btn = new KeyboardButton()
        {
          text = ("TelegramRequestLocation"),
          request_location = true
        };

        var btnList = new List<KeyboardButton>();
        btnList.Add(btn);
        reply_markup.keyboard.Add(btnList);
        msg.reply_markup = reply_markup;
        var replay = await sender.Send(msg);
        var deserializedClass = JsonSerializer.Deserialize<TelegramSingleUpdate>(replay);
        return deserializedClass;
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
      }

      return null;
    }

    public async Task DoWork()
    {
      TelegramSender sender = new TelegramSender();
      long offset = 0;

      while (!_cancellationToken.IsCancellationRequested)
      {
        await Task.Delay(5000);

        try
        {
          var replay = await sender.GetUpdates(_botId, offset);

          if (replay == null || !replay.ok || replay.result == null)
          {
            continue;
          }

          if (replay.result.Count == 0)
          {
            continue;
          }

          Console.WriteLine($"Received {replay.result.Count} updates");

          foreach (var r in replay.result)
          {
            offset = r.update_id + 1;

            if (r.callback_query != null && r.callback_query.data != null)
            {
              ProcessCallback(r);
            }
            if (r.edited_message != null)
            {
              await ProcessEditedMessage(r);
            }
            if (r.message != null)
            {
              if (r.message.photo != null)
              {
                await ProcessPhoto(r, _botId, _chatId);
              }

              if (r.message.location != null)
              {
                 await GrpcSendPoint(r.message);
              }

              if (r.message.reply_to_message != null)
              {
                ProcessReplay(r);
              }
            }
          }

          if (_somethingChanged)
          {
            // Send the event to clients.
            _somethingChanged = false;
          }
        }
        catch (Exception ex)
        {
          Console.WriteLine(ex.Message);
        }        
      }
    }

    public void Dispose()
    {
      if (_client != null)
      {
        _client.Dispose();
      }
    }

    private async Task GrpcSendPoint(Message message)
    {
      var figs = new TrackPointsProto();
      var track = new TrackPointProto();
      figs.Tracks.Add(track);

      track.Figure = new ProtoGeoObject();
      var fig = track.Figure;

      fig.Location = new ProtoGeometry();

      fig.Location.Type = "Point";

      DateTime timestamp = DateTime.UtcNow;


      if (message.edit_date != null)
      {
        timestamp = DateTime.UnixEpoch.AddSeconds(message.edit_date.Value);
      }
      else if (message.date != null)
      {
        timestamp = DateTime.UnixEpoch.AddSeconds(message.date.Value);
      }

      track.Timestamp = Timestamp.FromDateTime(timestamp);

      fig.Location.Coord.Add(new ProtoCoord()
      {
        Lat = message.location.latitude,
        Lon = message.location.longitude
      });


      track.ExtraProps.Add(new ProtoObjExtraProperty()
      {
        PropName = "track_name",
        StrVal = "lisa_alert"
      });
      
      track.ExtraProps.Add(new ProtoObjExtraProperty()
      {
        PropName = "from.username",
        StrVal = message.from.username
      });
      track.ExtraProps.Add(new ProtoObjExtraProperty()
      {
        PropName = "from.id",
        StrVal = message.from.id.ToString()
      });
      track.ExtraProps.Add(new ProtoObjExtraProperty()
      {
        PropName = "chat.title",
        StrVal = message.chat.title
      });

      track.ExtraProps.Add(new ProtoObjExtraProperty()
      {
        PropName = "chat.id",
        StrVal = message.chat.id.ToString()
      });

      try
      {
        await _client.UpdateTracks(figs);
      }
      catch(Exception ex) 
      {
        Console.WriteLine(ex.Message);
      }
    }
  }
}
