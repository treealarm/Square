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
    private CancellationToken _cancellationToken = new CancellationToken();
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
    public Task StartAsync(CancellationToken stoppingToken)
    {
      _logger.LogInformation("Timed Hosted Service running.");

      _timer = new Task(() => DoWork(), _cancellationToken);
      _timer.Start();

      return Task.CompletedTask;
    }

    private async void DoWork()
    {
      _logger.LogInformation(
              "Timed Hosted Service is working.");

      await _levelService.Init();
      await _stateService.Init();

      while (!_cancellationToken.IsCancellationRequested)
      {
        await Task.Delay(1000);
        continue;
      }
    }

    public Task StopAsync(CancellationToken stoppingToken)
    {
      _logger.LogInformation("Timed Hosted Service is stopping.");
      _timer?.Wait();

      return Task.CompletedTask;
    }

    public void Dispose()
    {
      _timer?.Dispose();
    }
  }
}
