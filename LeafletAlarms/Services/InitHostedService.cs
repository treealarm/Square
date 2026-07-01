using Domain;
using LeafletAlarms.Authentication;

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

    // Прогреваем JWT signing key, но НЕ блокируя на этом StartAsync: ASP.NET Core не начинает
    // принимать НИКАКОЙ HTTP-трафик (включая [AllowAnonymous] /api/Auth/customer_login,
    // которому этот ключ вообще не нужен) пока все IHostedService.StartAsync не завершатся.
    // Раньше ключ к тому же лениво грузился внутри IssuerSigningKeyResolver через блокирующий
    // .Result на каждый авторизованный запрос — той проблемы это уже не касается (ключ
    // кешируется в _securityKey), но awaiting retry-цикла прямо тут добавлял до 30×2с к
    // запуску ВСЕГО сервера. Bearer-эндпоинты и так корректно отвечают 401, пока ключ не
    // прогрелся (см. IssuerSigningKeyResolver) — отдельного ожидания тут не нужно.
    var jwtOptions = _serviceProvider.GetRequiredService<ConfigureJwtBearerOptions>();
    _ = Task.Run(async () =>
    {
      var keyLoaded = await jwtOptions.EnsureKeyLoadedAsync(
        maxAttempts: 30,
        retryDelay: TimeSpan.FromSeconds(2),
        cancellationToken);

      if (!keyLoaded)
      {
        _logger.LogWarning("JWT signing key was not loaded from Keycloak at startup. " +
          "Authenticated requests will return 401 until the key becomes available.");
      }
    }, cancellationToken);

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
