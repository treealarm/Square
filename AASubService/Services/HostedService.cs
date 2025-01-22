using Analytics.Api.Media;
using Analytics.Api.Stream;
using Domain.PubSubTopics;
using Domain.ServiceInterfaces;
using Domain.Values;
using System.Text;
using System.Text.Json;
using static Google.Rpc.Context.AttributeContext.Types;

namespace AASubService
{
  internal class HostedService : IHostedService, IDisposable
  {
    private Task? _timer;
    private CancellationTokenSource _cancellationToken = new CancellationTokenSource();

    private readonly ISubService _sub;
   
    public HostedService(
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
      await _sub.Subscribe(Topics.OnValuesChanged, OnValuesChanged);
    }

    async Task IHostedService.StopAsync(CancellationToken cancellationToken)
    {
      await _sub.Unsubscribe(Topics.OnValuesChanged, OnValuesChanged);

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

    private const int QUEUE_LIMIT = 100;
    private readonly object _mutex = new object();
    private readonly Queue<KeyValuePair<ulong, Sample>> _analyticQueue = new();
    private MediaDescriptor _descriptor;

    public async Task OnAAMessage(string channel, string message)
    {
      // Преобразуем строку в StreamMessage
      StreamMessage streamMessage;
      try
      {
        streamMessage = StreamMessage.Parser.ParseFrom(Encoding.UTF8.GetBytes(message));
      }
      catch
      {
        throw new ArgumentException("Failed to parse request");
      }

      lock (_mutex)
      {
        // Проверяем лимит очереди
        if (_analyticQueue.Count > QUEUE_LIMIT)
        {
          // Сообщение об отклонении из-за переполнения
          Console.WriteLine("Queue limit reached. Dropping message.");
          return;
        }
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

        // Обновляем дескриптор в потокобезопасной манере
        lock (_mutex)
        {
          _descriptor = descriptor;
        }
      }
      else if (streamMessage.PayloadCase == StreamMessage.PayloadOneofCase.MediaSample)
      {
        var mediaSample = streamMessage.MediaSample;

        if (mediaSample.Content.IsEmpty)
        {
          throw new ArgumentException("Media sample content is empty");
        }

        // Добавляем образец в очередь
        lock (_mutex)
        {
          var sample = new Sample(ToTimestamp(mediaSample.Pts), mediaSample.Content.ToByteArray());
          _analyticQueue.Enqueue(new KeyValuePair<ulong, Sample>(streamMessage.SequenceNumber, sample));
        }
      }
      else
      {
        throw new ArgumentException("Unknown message type");
      }

      Console.WriteLine("Message processed successfully");
    }

    // Метод преобразования временной метки
    private DateTime ToTimestamp(Google.Protobuf.WellKnownTypes.Timestamp timestamp)
    {
      return timestamp.ToDateTime();
    }

    // Пример структуры для образца
    private record Sample(DateTime Timestamp, byte[] Content);

  }
}
