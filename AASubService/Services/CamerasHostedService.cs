﻿
using IntegrationUtilsLib;
using LeafletAlarmsGrpc;

namespace AASubService
{
  internal class CamerasHostedService : IHostedService, IDisposable
  {

    private Task? _timer;
    private CancellationTokenSource _cancellationToken = new CancellationTokenSource();
    private IntegrationSync _sync = new IntegrationSync();
    public CamerasHostedService(
    )
    {
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
