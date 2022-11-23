using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LeafletAlarms.Services
{
  public class LogicProcessorHost : IHostedService, IDisposable
  {
    private Task _timer;
    private CancellationTokenSource _cancellationToken = new CancellationTokenSource();

    public void Dispose()
    {
      _timer?.Dispose();
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
      _timer = new Task(() => DoWork(), _cancellationToken.Token);
      _timer.Start();

      return Task.CompletedTask;
    }

    private async void DoWork()
    {
      while (!_cancellationToken.IsCancellationRequested)
      {
        await Task.Delay(1000);
        continue;
      }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
      _cancellationToken.Cancel();
      _timer?.Wait();

      return Task.CompletedTask;
    }
  }
}
