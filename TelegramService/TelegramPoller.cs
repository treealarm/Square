using System.Text.Json;

namespace TelegramService
{
  internal class TelegramPoller
  {
    private string _botId;
    private string _chatId;

    private CancellationToken _cancellationToken = new CancellationToken();
    private bool _somethingChanged = false;
    private List<TelegramLocationRequest> _locationRequests = new List<TelegramLocationRequest>();
    private TelegramPoller() { }
    public TelegramPoller(string botId, string chatId)
    {
      _botId = botId;
      _chatId = chatId;
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

    private void ProcessEditedMessage(Result r)
    {
      var edited_message = r.edited_message;
      
      if (edited_message?.location != null)
      {

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
              ProcessEditedMessage(r);
            }
            if (r.message != null)
            {
              if (r.message.photo != null)
              {
                await ProcessPhoto(r, _botId, _chatId);
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
  }
}
