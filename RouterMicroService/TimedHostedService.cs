using Dapr.Client;
using DbLayer;
using DbLayer.Models;
using DbLayer.Services;
using Domain;
using Domain.GeoDBDTO;
using Domain.ServiceInterfaces;
using Domain.StateWebSock;
using Microsoft.Extensions.Options;
using System.Threading;
using static DbLayer.Models.DBRoutLine;

namespace RouterMicroService
{
  public class TimedHostedService : IHostedService, IDisposable
  {
    private int executionCount = 0;
    private readonly ILogger<TimedHostedService> _logger;
    private Task? _timer;
    private CancellationTokenSource _cancellationTokenSource
      = new CancellationTokenSource();
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

      _timer = new Task(() => DoWork(), _cancellationTokenSource.Token);
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

      while (!_cancellationTokenSource.IsCancellationRequested)
      {
        if (!_router.IsMapExist(_routeInstanse))
        {
          await Task.Delay(1000);
          continue;
        }        

        var notProcessed = await _routService.GetNotProcessedAsync(100);

        if (notProcessed.Count == 0)
        {
          await Task.Delay(1000);
          continue;
        }

        List<RoutLineDTO> routes = new List<RoutLineDTO>();

        foreach (var routLine in notProcessed)
        {
          var res = await ProcessSingleRout(routLine);
          if (res)
          {
            routes.Add(routLine);
          }          
        }

        await _routService.DeleteManyAsync(notProcessed.Select(t => t.id).ToList());

        if (routes.Count > 0)
        {
          await _routService.InsertManyAsync(routes);
        }
      }      
    }

    private async Task<bool> ProcessSingleRout(RoutLineDTO routLine)
    {
      try
      {
        var track2 = await _tracksService.GetByIdAsync(routLine.id_end);

        if (track2 == null)
        {
          return false;
        }

        var track1 = await _tracksService.GetLastAsync(track2.figure.id, track2.timestamp);

        if (track1 == null)
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

        if (routRet != null)
        {
          routRet.Insert(0, p1);
          routRet.Add(p2);

          routLine.figure.location = new GeometryPolylineDTO()
          {
            coord = routRet
          };

          routLine.ts_start = track1.timestamp;
          routLine.id_start = track1.id;
          routLine.processed = RoutLineDTO.EntityType.processsed;
        }
        else
        {
          return false;
        }
      }
      catch(Exception ex)
      {
        Console.WriteLine(ex.ToString());
        return false;
      }

      return true;
    }

    public Task StopAsync(CancellationToken stoppingToken)
    {
      _logger.LogInformation("Timed Hosted Service is stopping.");
      _cancellationTokenSource.Cancel();
      _timer?.Wait();

      return Task.CompletedTask;
    }

    public void Dispose()
    {
      _timer?.Dispose();
    }
  }
}
