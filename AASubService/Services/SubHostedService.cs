
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
    }
  }
}
