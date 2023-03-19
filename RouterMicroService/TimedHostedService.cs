using Dapr.Client;
using DbLayer;
using DbLayer.Models;
using DbLayer.Services;
using Domain;
using Domain.GeoDBDTO;
using Domain.GeoDTO;
using Domain.NonDto;
using Domain.ServiceInterfaces;
using Domain.StateWebSock;
using Microsoft.Extensions.Options;
using PubSubLib;
using System.Text.Json;
using System.Threading;
using static DbLayer.Models.DBRoutLine;

namespace RouterMicroService
{
  public class TimedHostedService : BackgroundService
  {
    private int executionCount = 0;
    private readonly ILogger<TimedHostedService> _logger;
    private CancellationTokenSource _cancellationTokenSource
      = new CancellationTokenSource();
    private IRoutService _routService;
    private ITrackRouter _router;
    private readonly DaprClient _daprClient;
    private string _routeInstanse;
    private readonly ITrackService _tracksService;
    private IPubSubService _pubsub;

    public TimedHostedService(
      ILogger<TimedHostedService> logger,
      ITrackRouter router,
      IRoutService routService,
      DaprClient daprClient,
      IOptions<RoutingSettings> routingSettings,
      ITrackService tracksService,
      IPubSubService pubsub
     )
    {
      _routService = routService;
      _tracksService = tracksService;
      _router = router;
      _logger = logger;
      _daprClient = daprClient;
      _routeInstanse = routingSettings.Value.RouteInstanse;
      _pubsub = pubsub;
  }

    // 
    public async override Task StartAsync(CancellationToken cancellationToken)
    {
      _logger.LogInformation("Timed Hosted Service running.");
      await _pubsub.Subscribe(Topics.OnRequestRoutes, OnRequestRoutes);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
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
        await Task.Delay(1);

        if (!_router.IsMapExist(_routeInstanse))
        {
          await Task.Delay(1000);
          continue;
        }

        List<RoutLineDTO> notProcessed = new List<RoutLineDTO>();

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

    public async override Task StopAsync(CancellationToken cancellationToken)
    {
      await _pubsub.Unsubscribe(Topics.OnRequestRoutes, OnRequestRoutes);
      _logger.LogInformation("Timed Hosted Service is stopping.");
      _cancellationTokenSource.Cancel();
    }

    private void OnRequestRoutes(string channel, string message)
    {
      var ev = JsonSerializer.Deserialize<List<string>>(message);

      if (ev == null)
      {
        return;
      }
    }

    private async Task BuildRoutes(List<TrackPointDTO> trackPointsInserted)
    {
      var routs = new List<RoutLineDTO>();

      foreach (var trackPoint in trackPointsInserted)
      {
        if (trackPoint.figure.location is not GeometryCircleDTO)
        {
          continue;
        }

        {
          var newPoint = trackPoint;
          var newRout = new RoutLineDTO();

          newRout.id = newPoint.id;
          newRout.figure = new GeoObjectDTO();
          newRout.figure.id = newPoint.figure.id;
          newRout.figure.zoom_level = newPoint.figure.zoom_level;

          newRout.id_end = newPoint.id;
          newRout.ts_end = newPoint.timestamp;
          routs.Add(newRout);
        }
      }

      if (routs.Count > 0)
      {
        await _routService.InsertManyAsync(routs);
      }
    }
  }
}
