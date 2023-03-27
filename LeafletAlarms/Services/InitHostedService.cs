using Domain.GeoDBDTO;
using Domain.ServiceInterfaces;
using Domain.StateWebSock;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Threading;

namespace LeafletAlarms.Services
{
  public class InitHostedService : IHostedService, IDisposable
  {
    private readonly ILogger<InitHostedService> _logger;
    private Task _timer;
    private CancellationTokenSource _cancellationToken = new CancellationTokenSource();
    private ILevelService _levelService;
    private IStateService _stateService;
    public InitHostedService(
      ILogger<InitHostedService> logger,
      ILevelService lService,
      IStateService sService
     )
    {
      _stateService = sService;
      _levelService = lService;
      _logger = logger;
    }

    // 
    public async Task StartAsync(CancellationToken stoppingToken)
    {
      _logger.LogInformation("Timed Hosted Service running.");

      _timer = new Task(() => DoWork(), _cancellationToken.Token);
      _timer.Start();
      await Task.Delay(0);
    }

    private async void DoWork()
    {
      _logger.LogInformation(
              "Timed Hosted Service is working.");

      await _levelService.Init();
      await _stateService.Init();
      var curDate = DateTime.Now;

      while (!_cancellationToken.IsCancellationRequested)
      {
        await Task.Delay(1000);

        if (DateTime.Now - curDate > TimeSpan.FromMinutes(1))
        {
          curDate = DateTime.Now;
          Console.WriteLine($"GC Collect");
          GC.Collect();
        }


        continue;
      }
    }

    public async Task StopAsync(CancellationToken stoppingToken)
    {
      _cancellationToken.Cancel();
      _logger.LogInformation("Timed Hosted Service is stopping.");
      _timer?.Wait();
      await Task.Delay(0);
    }

    public void Dispose()
    {
      _timer?.Dispose();
    }
  }
}
