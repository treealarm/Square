
using Domain;
using IntegrationUtilsLib;
using LeafletAlarmsGrpc;
using System.Text.Json;

namespace AASubService
{
  internal class CamerasHostedService : IHostedService, IDisposable
  {

    private Task? _timer;
    private CancellationTokenSource _cancellationToken = new CancellationTokenSource();
    private IntegrationSync _sync = new IntegrationSync();
    private readonly ISubService _sub;
    private string? _topic_name = null;
    public CamerasHostedService(ISubService sub
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
    }

    async Task IHostedService.StopAsync(CancellationToken cancellationToken)
    {
      if (!string.IsNullOrEmpty(_topic_name))
      {
        await _sub.Unsubscribe(_topic_name, OnUpdateIntegros);
      }
      
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
      await Task.Delay(10000);
      await _sync.InitMainObject(_cancellationToken.Token);
      var types = new IntegroTypesProto();

      const string CamStr = "cam";

      var typeMain = new IntegroTypeProto()
      {
        IType = IntegrationSync.MainStr
      };
      typeMain.Children.Add(new IntegroTypeChildProto()
      { 
        ChildIType = "cam" 
      });

      var typeCam = new IntegroTypeProto()
      {
        IType = CamStr
      };
      types.Types_.Add(typeMain);
      types.Types_.Add(typeCam);

      await _sync.InitTypes(types, _cancellationToken.Token);

      _topic_name = $"{Topics.OnUpdateIntegros}_{_sync.MainIntegroObj!.IName}";
      await _sub.Subscribe(_topic_name, OnUpdateIntegros);

      while (!_cancellationToken.IsCancellationRequested)
      {
        await Task.Delay(1000);        
      }
    }

    public async Task OnUpdateIntegros(string channel, byte[] message)
    {
      var ids = JsonSerializer.Deserialize<List<string>>(message);

      if (ids == null || ids.Count == 0)
      {
        return;
      }
      await Task.Delay(0);
    }
  }
}
