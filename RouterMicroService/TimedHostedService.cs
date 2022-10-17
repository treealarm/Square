using Dapr.Client;
using DbLayer;
using DbLayer.Services;
using Domain;
using Domain.GeoDBDTO;
using Domain.ServiceInterfaces;
using Domain.StateWebSock;
using Microsoft.Extensions.Options;

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
    private readonly DaprClient _daprClient;
    private string _routeInstanse;
    private readonly ITrackService _tracksService;

    public TimedHostedService(
      ILogger<TimedHostedService> logger,
      ITrackRouter router,
      IRoutService routService,
      DaprClient daprClient,
      IOptions<RoutingSettings> routingSettings,
      ITrackService tracksService
     )
    {
      _routService = routService;
      _tracksService = tracksService;
      _router = router;
      _logger = logger;
      _daprClient = daprClient;
      _routeInstanse = routingSettings.Value.RouteInstanse;
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
      //using (StreamWriter sr = new StreamWriter(@"/osm_data/tmp1234.txt"))
      //{
      //  sr.WriteLine(DateTime.Now.ToString());
      //}

      var count = Interlocked.Increment(ref executionCount);
      _logger.LogInformation(
              "Timed Hosted Service is working. Count: {Count}", count);

      while (!_cancellationToken.IsCancellationRequested)
      {
        if (!_router.IsMapExist(_routeInstanse))
        {
          await Task.Delay(1000);
          continue;
        }        

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
      try
      {
        var track1 = await _tracksService.GetByIdAsync(routLine.id_end);

        if (track1 == null)
        {
          return false;
        }

        var track2 = await _tracksService.GetLastAsync(track1.figure.id, track1.timestamp);

        if (track2 == null)
        {
          return false;
        }

        var coords = new List<Geo2DCoordDTO>();
        var p1 = (track1.figure.location as GeometryCircleDTO)?.coord;
        var p2 = (track2.figure.location as GeometryCircleDTO)?.coord;

        if (p1 == null || p2 == null)
        {
          return false;
        }

        coords.Add(p1);
        coords.Add(p2);

        var routRet = await _router.GetRoute(_routeInstanse, coords);

        if (routRet != null && routRet.Count > 0)
        {
          routRet.Insert(0, p1);
          routRet.Add(p2);

          routLine.figure.location = new GeometryPolylineDTO()
          {
            coord = routRet
          };          
        }
      }
      catch(Exception ex)
      {        
        return false;
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
