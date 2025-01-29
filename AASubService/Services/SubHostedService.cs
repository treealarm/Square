using Analytics.Api.Media;
using Analytics.Api.Stream;
using Domain.ServiceInterfaces;
using Domain.Values;
using Google.Protobuf.WellKnownTypes;
using GrpcDaprLib;
using LeafletAlarmsGrpc;
using System.Text.Json;

namespace AASubService
{
  internal class SubHostedService : IHostedService, IDisposable
  {
    private static GrpcUpdater? _client;
    private static Dictionary<string, string> _idsCash = new Dictionary<string, string>();
    private static readonly object _lock = new object(); // Для синхронизации

    // Метод для доступа к клиенту
    public static GrpcUpdater Client
    {
      get
      {
        // Проверяем, если клиент не существует или мертв, создаем новый
        if (_client == null || _client.IsDead)
        {
          lock (_lock)
          {
            if (_client == null || _client.IsDead)
            {
              _client = new GrpcUpdater();  // Инициализация нового клиента
            }
          }
        }
        return _client;
      }
    }

    private Task? _timer;
    private CancellationTokenSource _cancellationToken = new CancellationTokenSource();
    private readonly ISubService _sub;

    public SubHostedService(
      ISubService sub
    )
    {
      _sub = sub;
    }

    async Task IHostedService.StartAsync(CancellationToken cancellationToken)
    {
      await Task.Delay(1);
      // Start timer after processing initial states.
      _timer = new Task(() => DoWork(), _cancellationToken.Token);
      _timer.Start();
     // await _sub.Subscribe(Topics.OnValuesChanged, OnValuesChanged);
      await _sub.Subscribe("lukich", "media", OnAAMessage);
    }

    async Task IHostedService.StopAsync(CancellationToken cancellationToken)
    {
      //await _sub.Unsubscribe(Topics.OnValuesChanged, OnValuesChanged);
      await _sub.Unsubscribe("lukich", "media", OnAAMessage);

      _cancellationToken.Cancel();
      _timer?.Wait();

      await Task.Delay(1);
    }

    void IDisposable.Dispose()
    {
      _timer?.Dispose();
    }


    private async void DoWork()
    {
      while (!_cancellationToken.IsCancellationRequested)
      {
        await Task.Delay(1000);
      }
    }
    public async Task OnValuesChanged(string channel, byte[] message)
    {
      var state = JsonSerializer.Deserialize<List<ValueDTO>>(message);
      if (state == null)
      {
        return;
      }
      await Task.Delay(1);
    }

    public async Task OnAAMessage(string channel, byte[] message)
    {
      await Task.Delay(0);
      // Преобразуем строку в StreamMessage
      StreamMessage streamMessage;
      try
      {
        streamMessage = StreamMessage.Parser.ParseFrom(message);
      }
      catch(Exception ex)
      {
        Console.WriteLine(ex.ToString());
        throw;
      }


      if (streamMessage.PayloadCase == StreamMessage.PayloadOneofCase.StreamDescriptor)
      {
        var descriptor = streamMessage.StreamDescriptor.MediaDescriptor;

        if (descriptor.DataFormat == null)
        {
          throw new ArgumentException("Video format not found");
        }

        if (descriptor.DataFormat.Format != DataFormat.Types.Format.Analytic)
        {
          throw new NotSupportedException("Raw video format not yet implemented");
        }
       
      }
      else if (streamMessage.PayloadCase == StreamMessage.PayloadOneofCase.MediaSample)
      {
        var mediaSample = streamMessage.MediaSample;

        if (mediaSample.Content.IsEmpty)
        {
          throw new ArgumentException("Media sample content is empty");
        }

        // Сохраняем изображение, если оно есть
        var contentBytes = mediaSample.Content.ToByteArray();

        // Предполагаем, что контент — это JPEG-изображение
        try
        {
          //var fileName = $"image_{DateTime.UtcNow:yyyyMMdd_HHmmssfff}.jpeg";
          //var filePath = Path.Combine("images", fileName);
          //Directory.CreateDirectory("images");
          //await File.WriteAllBytesAsync(filePath, contentBytes);
          //Console.WriteLine($"Image saved: {filePath}");

          var client = Client;
          if (client != null) 
          {
            var newEv = new EventProto();
            newEv.Timestamp = Timestamp.FromDateTime(DateTime.UtcNow);

            newEv.Meta = new EventMetaProto();

            newEv.EventPriority = (int)LogLevel.Critical;
            newEv.EventName = "lukich";

            newEv.Meta.ExtraProps.Add(new ProtoObjExtraProperty()
            {
              PropName = "lukich1",
              StrVal = $"lukich2"
            });

            newEv.Meta.NotIndexedProps.Add(new ProtoObjExtraProperty()
            {
              PropName = "license_image",
              StrVal = Convert.ToBase64String(contentBytes),
              VisualType = "base64image_fs"
            });
            newEv.ObjectId = "64270c097a71c88757377dcf";

            var events = new EventsProto();
            events.Events.Add(newEv);
            var result = await client.AddEvents(events);
          }
        }
        catch (Exception ex)
        {
          Console.WriteLine($"Failed to save image: {ex.Message}");
        }

      }

      else
      {
        throw new ArgumentException("Unknown message type");
      }
    }

    // Метод преобразования временной метки
    private DateTime ToTimestamp(Google.Protobuf.WellKnownTypes.Timestamp timestamp)
    {
      return timestamp.ToDateTime();
    }
  }
}
