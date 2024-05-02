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
using System.Text.Json.Serialization;
using System.Threading;
using static DbLayer.Models.DBRoutLine;

namespace RouterMicroService
{
  public class TimedHostedService : BackgroundService
  {
    private int executionCount = 0;
    private readonly ILogger<TimedHostedService> _logger;

    private IRoutService _routService;
    private ITrackRouter _router;
    private readonly DaprClient _daprClient;
    private string _routeInstanse;
    private readonly ITrackService _tracksService;
    private IPubSubService _pubsub;
    private IIdsQueue _idsToProcess = new ConcurentHashSet();
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
      await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
      var count = Interlocked.Increment(ref executionCount);
      _logger.LogInformation(
              "Timed Hosted Service is working. Count: {Count}", count);

      while (!stoppingToken.IsCancellationRequested)
      {
        await Task.Delay(1);

        try
        {
          if (!_router.IsMapExist(_routeInstanse))
          {
            await Task.Delay(1000);
            continue;
          }

          var idsToProcess = _idsToProcess.GetIds();
          var processed = (await _routService.GetProcessedIdsAsync(idsToProcess)).ToHashSet();
          var notProcessed = idsToProcess.Where(i => !processed.Contains(i)).ToHashSet();

          if (notProcessed.Count == 0)
          {
            await Task.Delay(1000);
            continue;
          }

          List<RoutLineDTO> routes = new List<RoutLineDTO>();

          foreach (var endId in notProcessed)
          {
            try
            {
              var res = await ProcessSingleRout(endId);

              if (res != null)
              {
                routes.Add(res);
              }
            }
            catch (Exception ex)
            {
              Console.WriteLine(ex.ToString());
            }
          }

          if (routes.Count > 0)
          {
            await _routService.InsertManyAsync(routes);
            var ids = routes.Select(r => r.id_end);
            _pubsub.PublishNoWait(Topics.NewRoutBuilt, JsonSerializer.Serialize(ids));
          }
        }
        catch (Exception ex)
        {
          Console.WriteLine(ex.ToString());
        }
      }
    }

    private async Task<RoutLineDTO> ProcessSingleRout(string id_end)
    {
      RoutLineDTO routLine = new RoutLineDTO();
      routLine.id_end = id_end;

      try
      {
        var routRet = new List<Geo2DCoordDTO>();
        var track2 = await _tracksService.GetByIdAsync(id_end);

        if (track2 == null)
        {
          return routLine;
        }
        routLine.ts_end = track2.timestamp;
        routLine.id = track2.id;

        var track1 = await _tracksService.GetLastAsync(track2.figure.id, track2.timestamp);

        if (track1 == null)
        {
          routLine.ts_start = track2.timestamp;
          routLine.id_start = track2.id;
          routLine.figure = track2.figure;

          return routLine;
        }

        routLine.ts_start = track1.timestamp;
        routLine.id_start = track1.id;

        var coords = new List<Geo2DCoordDTO>();
        var p1 = (track1.figure.location as GeometryCircleDTO)?.coord;
        var p2 = (track2.figure.location as GeometryCircleDTO)?.coord;

        if (p1 == null || p2 == null)
        {
          return routLine;
        }

        coords.Add(p1);
        coords.Add(p2);

        routRet = await _router.GetRoute(_routeInstanse, "car", coords);

        if (routRet == null)
        {
          routRet = new List<Geo2DCoordDTO>();
        }

        if (routRet != null)
        {
          routRet.Insert(0, p1);
          routRet.Add(p2);

          routLine.figure = new GeoObjectDTO();
          routLine.figure.location = new GeometryPolylineDTO()
          {
            coord = routRet,
          };

          routLine.figure.id = track2.id;
          routLine.figure.zoom_level = track2.figure.zoom_level;
        }
      }
      catch(Exception ex)
      {
        Console.WriteLine(ex.ToString());
      }

      return routLine;
    }

    public async override Task StopAsync(CancellationToken cancellationToken)
    {
      await _pubsub.Unsubscribe(Topics.OnRequestRoutes, OnRequestRoutes);
      _logger.LogInformation("Timed Hosted Service is stopping.");
      await base.StopAsync(cancellationToken);
    }

    private void OnRequestRoutes(string channel, string message)
    {
      var ids = JsonSerializer.Deserialize<List<string>>(message);

      if (ids == null)
      {
        return;
      }
      _idsToProcess.AddIds(ids);
    }
  }
}
