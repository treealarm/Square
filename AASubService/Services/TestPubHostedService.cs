using Analytics.Api.Primitives;
using Analytics.Api.Stream;
using Domain.ServiceInterfaces;
using Google.Protobuf;
using ImageLib;

namespace AASubService
{
  internal class TestPubHostedService : IHostedService, IDisposable
  {
    private Task? _timer;
    private CancellationTokenSource _cancellationToken = new CancellationTokenSource();

    private readonly IPubService _pub;
    public TestPubHostedService(
      IPubService pub
    )
    {
      _pub = pub;
    }

    async Task IHostedService.StartAsync(CancellationToken cancellationToken)
    {
      await Task.Delay(1);
      // Start timer after processing initial states.
      _timer = new Task(() => DoWork(), _cancellationToken.Token);
      _timer.Start();
    }

    async Task IHostedService.StopAsync(CancellationToken cancellationToken)
    {

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

        // Генерация картинки JPEG
        var jpegImage = ImageService.GenerateJpegImage();

        // Выделение области на картинке (например, прямоугольник)
        var rectangle = new Rectangle
        {
          L = 0.1f, // Отступ от левого края (10% от ширины)
          T = 0.1f, // Отступ от верхнего края (10% от высоты)
          W = 0.5f, // Ширина прямоугольника (50% от ширины картинки)
          H = 0.5f  // Высота прямоугольника (50% от высоты картинки)
        };

        // Создаём сообщение для MediaSample
        var msg = new StreamMessage
        {
          SequenceNumber = (ulong)DateTime.UtcNow.Ticks,
          AckSequenceNumber = 0,
          MediaSample = new MediaSample
          {
            Duration = new Google.Protobuf.WellKnownTypes.Duration { Seconds = 1 },
            Pts = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow),
            Dts = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow.AddSeconds(-1)),
            Pos = 100,
            Content = ByteString.CopyFrom(jpegImage)  // Передаём JPEG картинку как контент
          }
        };

        // Публикуем сообщение для media
        await _pub.Publish("lukich", "media", msg.ToByteArray());

        //// Создаём сообщение для аналитики (выделенная область)
        //var analyticSample = new AnalyticSample
        //{
        //  Inferences = { new Inference
        //    {
        //        InferenceId = "inference-12345",
        //        SequenceId = "sequence-001",

        //        Entity = new Entity
        //        {
        //          Tag = new Tag { Value = "object", Confidence = 0.99f },
        //          Attributes = 
        //          { new Attribute 
        //            { 
        //              Name = "type", 
        //              Confidence = 1.0f, 
        //              Text = "car" 
        //            } 
        //          },
        //          Box = rectangle  // Выделенная область на изображении
        //        }

        //    }}
        //};

        //// Публикуем сообщение аналитики
        //await _pub.Publish("lukich", "analytics", analyticSample);
      }
    }

    //private async void DoWork()
    //{
    //  while (!_cancellationToken.IsCancellationRequested)
    //  {
    //    await Task.Delay(1000);
    //    var msg = new StreamMessage();
    //    await _pub.Publish("lukich", "media", msg);
    //  }
    //}
  }
}
