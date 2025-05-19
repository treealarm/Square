
using Analytics.Api.Inferencing;
using Analytics.Api.Media;
using Analytics.Api.Primitives;
using Analytics.Api.Stream;
using Google.Protobuf;
using ImageLib;
using Attribute = Analytics.Api.Inferencing.Attribute;

namespace AASubService
{
  internal class TestPubHostedService : IHostedService, IDisposable
  {
    private Task? _timer;
    private CancellationTokenSource _cancellationToken = new CancellationTokenSource();
    ulong _mediaSequenceNumber = 2;
    ulong _analyticsSequenceNumber = 2;
    private readonly IPubServiceLu _pub;
    private readonly string _topic_name;
    public TestPubHostedService(
      IPubServiceLu pub
    )
    {
      _pub = pub;
      _topic_name = Environment.GetEnvironmentVariable("TOPIC_NAME") ?? "media";
      Console.WriteLine($"TestPubService TOPIC_NAME:{_topic_name}");
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

    private async Task PublishDescriptor(string streamId)
    {
      var descriptor = new StreamMessage
      {
        SequenceNumber = 1,
        AckSequenceNumber = 0,
        StreamDescriptor = new StreamDescriptor
        {
          StreamId = streamId,
          MediaDescriptor = new MediaDescriptor()
        }
      };
      await _pub.Publish(_topic_name, descriptor.ToByteArray());
    }

    private async Task PublishImage(string streamId)
    {
      var jpegImage = ImageService.GenerateJpegImage();
      var base64Image = Convert.ToBase64String(jpegImage);

      var mediaMsg = new StreamMessage
      {
        SequenceNumber = _mediaSequenceNumber++,
        AckSequenceNumber = 0,
        MediaSample = new MediaSample
        {
          Duration = new Google.Protobuf.WellKnownTypes.Duration { Seconds = 1 },
          Pts = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow),
          Dts = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow.AddSeconds(-1)),
          Pos = 100,
          Content = ByteString.CopyFromUtf8(base64Image)
        }
      };
      await _pub.Publish(_topic_name, mediaMsg.ToByteArray());
    }

    private async Task PublishAnalytic(string streamId)
    {
      var analyticSample = new AnalyticSample
      {
        Inferences =
        {
            new Inference
            {
                InferenceId = "inference-12345",
                SequenceId = "sequence-001",
                Entity = new Entity
                {
                    Tag = new Tag { Value = "object", Confidence = 0.99f },
                    Attributes =
                    {
                        new Attribute
                        {
                            Name = "type",
                            Confidence = 1.0f,
                            Text = "car"
                        }
                    },
                    Box = new Rectangle
                    {
                        L = 0.1f, T = 0.1f, W = 0.5f, H = 0.5f
                    }
                }
            }
        }
      };

      var analyticContent = Convert.ToBase64String(analyticSample.ToByteArray());

      var analyticMsg = new StreamMessage
      {
        SequenceNumber = _analyticsSequenceNumber++,
        AckSequenceNumber = 0,
        MediaSample = new MediaSample
        {
          Duration = new Google.Protobuf.WellKnownTypes.Duration { Seconds = 1 },
          Pts = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow),
          Content = ByteString.CopyFromUtf8(analyticContent)
        }
      };
      await _pub.Publish(_topic_name, analyticMsg.ToByteArray());
    }

    private async void DoWork()
    {
      var mediaStreamId = Guid.NewGuid().ToString();
      var analyticsStreamId = Guid.NewGuid().ToString();

      // Отправляем StreamDescriptor для каждого потока
      await PublishDescriptor( mediaStreamId);
      await PublishDescriptor(analyticsStreamId);
      

      while (!_cancellationToken.IsCancellationRequested)
      {
        await Task.Delay(1000);
        await PublishImage(mediaStreamId);
        await PublishAnalytic(analyticsStreamId);
      }
    }

  }
}
