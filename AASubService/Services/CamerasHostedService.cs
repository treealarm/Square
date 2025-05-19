
using Domain;

namespace AASubService
{
  internal class CamerasHostedService : IHostedService, IAsyncDisposable
  {

    private Task? _timer;
    private CancellationTokenSource _cancellationToken = new CancellationTokenSource();

    private readonly ICameraManager _cameraManager;
    public CamerasHostedService(ISubService sub, ICameraManager cameraManager)
    {
      _cameraManager = cameraManager;      
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

    public async ValueTask DisposeAsync()
    {
      _timer?.Dispose();
      await _cameraManager.DisposeAsync();
    }

    
    private async void DoWork()
    {
      await Task.Delay(10000);
      await _cameraManager.DoWork(_cancellationToken);
    }
  }
}
