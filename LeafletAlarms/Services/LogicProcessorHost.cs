using DbLayer.Services;
using Domain.GeoDTO;
using Domain.ServiceInterfaces;
using Domain.StateWebSock;
using LeafletAlarms.Services.Logic;
using Microsoft.Extensions.Hosting;
using OsmSharp.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LeafletAlarms.Services
{
  public class LogicProcessorHost : IHostedService, IDisposable
  {
    private Task _timer;
    private CancellationTokenSource _cancellationToken = new CancellationTokenSource();
    private PubSubService _pubsub;
    private ILogicProcessorService _logicProcessorService;
    private ILogicService _logicService;
    private IGeoService _geoService;
    private readonly ITrackService _tracksService;
    private List<TrackPointDTO> _listOfNewTracks = new List<TrackPointDTO>();
    private object _locker = new object();

    private Dictionary<string, TrackCounter> _trackCounters =
      new Dictionary<string, TrackCounter>();

    public LogicProcessorHost(
      PubSubService pubsub,
      ILogicProcessorService logicProcessorService,
      ILogicService logicService,
      IGeoService geoService,
      ITrackService tracksService
    )
    {
      _tracksService = tracksService;
      _logicProcessorService = logicProcessorService;
      _logicService = logicService;
      _geoService = geoService;
      _pubsub = pubsub;
    }    

    void OnUpdateTrackPosition(string channel,object message)
    {
      var list = message as List<TrackPointDTO>;

      if (list == null)
      {
        return;
      }

      lock(_locker)
      {
        _listOfNewTracks.AddRange(list);
      }           
    }

    public void Dispose()
    {
      _timer?.Dispose();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
      await Task.Delay(0);

      _pubsub.Subscribe("UpdateTrackPosition", OnUpdateTrackPosition);

      _timer = new Task(() => DoWork(), _cancellationToken.Token);
      _timer.Start();
    }

    private async void DoWork()
    {
      var logics = await _logicService.GetListAsync(null, true, 1000);
      Dictionary<string, List<GeoObjectDTO>> dicLogicToFigures
         = new Dictionary<string, List<GeoObjectDTO>>();

      DateTime curStart = DateTime.UtcNow;

      foreach (var logic in logics)
      {
        var geoFigs = await
            _geoService.GetGeoObjectsAsync(logic.figs.Select(f => f.id).ToList());

        dicLogicToFigures[logic.id] = geoFigs.Values.ToList();

        if (logic.logic == "counter")
        {
          _trackCounters[logic.id] = new TrackCounter(logic.id);
        }        
      }

      BoxTrackDTO box = new BoxTrackDTO();


      while (!_cancellationToken.IsCancellationRequested)
      {
        List<TrackPointDTO> listOfNewTracks;
        box.time_start = curStart;
        box.time_end = DateTime.UtcNow;

        foreach ( var zoneKeyVal in dicLogicToFigures)
        {
          var logic_id = zoneKeyVal.Key;
          box.zone = zoneKeyVal.Value.Select(f => f.location).ToList();

          TrackCounter trackCounter;
          _trackCounters.TryGetValue(logic_id, out trackCounter);

          if (trackCounter != null)
          {
            var inZones = trackCounter.GetInZones();

            if (inZones.Count > 0)
            {
              box.not_in_zone = true;
              box.ids = inZones;
              var tracksNotInZone = await _tracksService.GetTracksByBox(box);
              trackCounter.NotFound(tracksNotInZone);
            }              
          }

          box.ids = null;
          box.not_in_zone = false;
          listOfNewTracks = await _tracksService.GetTracksByBox(box);

          if (listOfNewTracks != null && listOfNewTracks.Count > 0)
          {
            var logic = logics.Where(l => l.id == logic_id).FirstOrDefault();

            if (trackCounter != null)
            {
              trackCounter.Found(listOfNewTracks);
            }
          }

        }

        curStart = box.time_end.Value;

        await Task.Delay(1000);

      }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
      _pubsub.Unsubscribe("UpdateTrackPosition", OnUpdateTrackPosition);
      _cancellationToken.Cancel();
      _timer?.Wait();

      return Task.CompletedTask;
    }
  }
}
