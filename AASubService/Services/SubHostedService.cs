
using Analytics.Api.Media;
using Analytics.Api.Stream;
using Google.Protobuf.WellKnownTypes;
using ImageLib;
using IntegrationUtilsLib;
using LeafletAlarmsGrpc;

namespace AASubService
{
  internal class SubHostedService : IHostedService, IDisposable
  {
    private static Dictionary<string, string> _idsCash = new Dictionary<string, string>();
    private static readonly object _lock = new object(); // Для синхронизации


    private Task? _timer;
    private CancellationTokenSource _cancellationToken = new CancellationTokenSource();
    private readonly ISubServiceLu _sub;
    private readonly string _topic_name;
    public SubHostedService(
      ISubServiceLu sub
    )
    {
      _sub = sub;
      _topic_name = Environment.GetEnvironmentVariable("TOPIC_NAME") ?? "media";
      {
        Console.WriteLine($"TestPubService TOPIC_NAME:{_topic_name}");
      }
    }

    async Task IHostedService.StartAsync(CancellationToken cancellationToken)
    {
      await Task.Delay(1);
      // Start timer after processing initial states.
      _timer = new Task(() => DoWork(), _cancellationToken.Token);
      _timer.Start();
      await _sub.Subscribe(_topic_name, OnAAMessage);
    }

    async Task IHostedService.StopAsync(CancellationToken cancellationToken)
    {
      //await _sub.Unsubscribe(Topics.OnValuesChanged, OnValuesChanged);
      await _sub.Unsubscribe(_topic_name, OnAAMessage);

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

    public async Task OnAAMessage(string channel, byte[] message)
    {
      await Task.Delay(0);
      return;
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

        

        // Предполагаем, что контент — это JPEG-изображение
        try
        {
          var client = Utils.ClientBase;
          if (client != null) 
          {
            var newEv = new EventProto();
            newEv.Timestamp = Timestamp.FromDateTime(DateTime.UtcNow);


            newEv.EventPriority = (int)LogLevel.Critical;
            newEv.EventName = "lukich";

            newEv.ExtraProps.Add(new ProtoObjExtraProperty()
            {
              PropName = "lukich1",
              StrVal = $"lukich2"
            });

            // Сохраняем изображение, если оно есть
            string base64Image = mediaSample.Content.ToStringUtf8();
            byte[] imageBytes = Convert.FromBase64String(base64Image);

            if (ImageService.IsValidImage(imageBytes))
            {
              newEv.ExtraProps.Add(new ProtoObjExtraProperty()
              {
                PropName = "license_image",
                StrVal = base64Image,
                VisualType = "base64image_fs"
              });
            }

            
            newEv.ObjectId = "64270c097a71c88757377dcf";

            var events = new EventsProto();
            events.Events.Add(newEv);
            var result = await client.Client.UpdateEventsAsync(events);
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
  }
}
