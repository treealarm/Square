using Domain;

public class InitHostedService : IHostedService, IDisposable
{
  private readonly ILogger<InitHostedService> _logger;
  private readonly IServiceProvider _serviceProvider;
  private Task _backgroundTask;
  private CancellationTokenSource _cts;

  public InitHostedService(
    ILogger<InitHostedService> logger,
    IServiceProvider serviceProvider)
  {
    _logger = logger;
    _serviceProvider = serviceProvider;
  }

  public async Task StartAsync(CancellationToken cancellationToken)
  {
    _logger.LogInformation("InitHostedService is starting...");

    // Инициализация сервисов в скоупе
    using (var scope = _serviceProvider.CreateScope())
    {
      var levelService = scope.ServiceProvider.GetRequiredService<ILevelService>();
      var stateService = scope.ServiceProvider.GetRequiredService<IStateService>();

      await levelService.Init();
      await stateService.Init();
    }

    // Запускаем фоновую задачу
    _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
    _backgroundTask = Task.Run(() => DoWorkAsync(_cts.Token), _cts.Token);

    _logger.LogInformation("InitHostedService started.");
  }

  public Task StopAsync(CancellationToken cancellationToken)
  {
    _logger.LogInformation("InitHostedService is stopping...");

    _cts?.Cancel();

    return _backgroundTask ?? Task.CompletedTask;
  }

  private async Task DoWorkAsync(CancellationToken token)
  {
    var curDate = DateTime.UtcNow;

    while (!token.IsCancellationRequested)
    {
      await Task.Delay(1000, token);

      if (DateTime.UtcNow - curDate > TimeSpan.FromMinutes(1))
      {
        curDate = DateTime.UtcNow;
        _logger.LogInformation("GC Collect");
        GC.Collect();
      }

      // пример: можно вызвать сервисы снова, если потребуется
      /*
      using var scope = _serviceProvider.CreateScope();
      var someScopedService = scope.ServiceProvider.GetRequiredService<ISomeScopedService>();
      await someScopedService.DoPeriodicCheck();
      */
    }
  }

  public void Dispose()
  {
    _cts?.Dispose();
  }
}
