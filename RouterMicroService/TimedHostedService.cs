using DbLayer;
using Domain.GeoDBDTO;
using Domain.ServiceInterfaces;
using Domain.StateWebSock;

namespace RouterMicroService
{
  public class TimedHostedService : IHostedService, IDisposable
  {
    private int executionCount = 0;
    private readonly ILogger<TimedHostedService> _logger;
    private Task? _timer;
    CancellationToken _cancellationToken = new CancellationToken();
    private IRoutService _routService;
    private ITrackRouter _router;
    public TimedHostedService(
      ILogger<TimedHostedService> logger,
      ITrackRouter router,
      IRoutService routService
     )
    {
      _routService = routService;
      _router = router;
      _logger = logger;
    }

    public Task StartAsync(CancellationToken stoppingToken)
    {
      _logger.LogInformation("Timed Hosted Service running.");

      _timer = new Task(() => DoWork(), _cancellationToken);
      _timer.Start();

      return Task.CompletedTask;
    }

    private async void DoWork()
    {
      while (!_cancellationToken.IsCancellationRequested)
      {
        if (!_router.IsMapExist(String.Empty))
        {
          await Task.Delay(1000);
          continue;
        }

        var count = Interlocked.Increment(ref executionCount);
        _logger.LogInformation(
                "Timed Hosted Service is working. Count: {Count}", count);

        var notProcessed = await _routService.GetNotProcessedAsync(1);

        if (notProcessed.Count == 0)
        {
          await Task.Delay(1000);
          continue;
        }

        foreach (var routLine in notProcessed)
        {
          var res = await ProcessSingleRout(routLine);
          routLine.processed = true;
          await _routService.UpdateAsync(routLine);
        }
      }      
    }

    private async Task<bool> ProcessSingleRout(RoutLineDTO routLine)
    {
      var coords = new List<Geo2DCoordDTO>();
      var p1 = (routLine.figure.location as GeometryPolylineDTO)?.coord.FirstOrDefault();
      var p2 = (routLine.figure.location as GeometryPolylineDTO)?.coord.LastOrDefault();

      if (p1 ==  null || p2 == null)
      {
        return false;
      }

      coords.Add(p1);      
      coords.Add(p2);

      var routRet = await _router.GetRoute(string.Empty, coords);

      if (routRet != null && routRet.Count > 0)
      {
        routRet.Insert(0, p1);
        routRet.Add(p2);
        var line = routLine.figure.location as GeometryPolylineDTO;

        if (line == null)
        {
          return false;
        }

        line.coord = routRet;
      }

      return true;
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
