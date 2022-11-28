using DbLayer.Services;
using Domain;
using Domain.GeoDTO;
using Domain.Logic;
using Domain.NonDto;
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
    private ILogicService _logicService;
    private IGeoService _geoService;
    private readonly IMapService _mapService;
    private readonly ITrackService _tracksService;
    private readonly IUtilService _utilService;
    private object _locker = new object();

    private Dictionary<string, TrackCounter> _trackCounters =
      new Dictionary<string, TrackCounter>();

    private TracksUpdatedEvent _rangeToProcess = new TracksUpdatedEvent();

    public LogicProcessorHost(
      PubSubService pubsub,
      ILogicService logicService,
      IGeoService geoService,
      ITrackService tracksService,
      IMapService mapService,
      IUtilService utilService
    )
    {
      _utilService = utilService;
      _mapService = mapService;
      _tracksService = tracksService;
      _logicService = logicService;
      _geoService = geoService;
      _pubsub = pubsub;

      _rangeToProcess.ts_start = DateTime.MaxValue;
      _rangeToProcess.ts_end = DateTime.MinValue;
    }    

    void OnUpdateTrackPosition(string channel,object message)
    {
      var ev = message as TracksUpdatedEvent;

      if (ev == null)
      {
        return;
      }

      lock(_locker)
      {
        if (_rangeToProcess.ts_start > ev.ts_start)
        {
          _rangeToProcess.ts_start = ev.ts_start;
        }

        if (_rangeToProcess.ts_end < ev.ts_end)
        {
          _rangeToProcess.ts_end = ev.ts_end;
        }

        if (_utilService.Compare(_rangeToProcess.id_start, ev.id_start) > 0)
        {
          _rangeToProcess.id_start = ev.id_start;
        }

        if (_utilService.Compare(_rangeToProcess.id_end, ev.id_end) < 0)
        {
          _rangeToProcess.id_end = ev.id_end;
        }
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

    private async Task OnUpdateCounterLogic(string logic_id)
    {
      TrackCounter trackCounter;

      if (_trackCounters.TryGetValue(logic_id, out trackCounter))
      {
        LogicTriggered triggeredVal = new LogicTriggered()
        {
          Count = trackCounter.GetInZonesCount(),
          LogicId = trackCounter.LogicId,
          LogicTextObjects = trackCounter.TextObjectsIds
        };

        if (trackCounter.TextObjectsIds != null && trackCounter.TextObjectsIds.Count > 0)
        {
          var updatedProps = new List<ObjPropsDTO>();

          foreach (var textObjId in trackCounter.TextObjectsIds)
          {
            var updatedObj = new ObjPropsDTO()
            {
              id = textObjId,
              extra_props = new List<ObjExtraPropertyDTO>()
              {
                new ObjExtraPropertyDTO()
                {
                  prop_name = "text",
                  str_val = triggeredVal.Count.ToString()
                }
              }
            };

            updatedProps.Add(updatedObj);
          }

          await _mapService.UpdatePropNotDeleteAsync(updatedProps);
        }

        _pubsub.Publish("LogicTriggered", triggeredVal);
      }
    }
    private async void DoWork()
    {
      var logics = await _logicService.GetListAsync(null, true, 1000);
      Dictionary<string, List<GeoObjectDTO>> dicLogicToFigures
         = new Dictionary<string, List<GeoObjectDTO>>();

      BoxTrackDTO box = new BoxTrackDTO();

      foreach (var logic in logics)
      {
        var textObjsIds = logic.figs
          .Where(f => f.group_id == "text")
          .Select(f => f.id)
          .ToHashSet();

        var logicObjs = logic.figs.Where(f => f.group_id != "text").ToList();

        var geoFigs = await
            _geoService.GetGeoObjectsAsync(logicObjs.Select(f => f.id).ToList());

        var zones = geoFigs.Values.ToList();
        dicLogicToFigures[logic.id] = zones;

        if (logic.logic == "counter")
        {
          var trackCounter = new TrackCounter(logic.id, textObjsIds);
          _trackCounters[logic.id] = trackCounter;
          box.zone = zones.Select(f => f.location).ToList();
          var objectsNowInZone = await _geoService.GetGeoAsync(box);
          
          var removeFigs = logic.figs.Select(f => f.id).ToHashSet();

          var initList = objectsNowInZone.Values
            .Select(f => f.id)
            .Where(d => !removeFigs.Contains(d))
            .ToList();
          trackCounter.InitZone(initList);

          await OnUpdateCounterLogic(logic.id);
        }        
      }


      while (!_cancellationToken.IsCancellationRequested)
      {
        bool bContinue = false;
        lock (_locker)
        {
          if (box.time_start == _rangeToProcess.ts_start &&
          box.time_end == _rangeToProcess.ts_end)
          {            
            bContinue = true;
          }

          box.time_start = _rangeToProcess.ts_start;
          box.time_end = _rangeToProcess.ts_end;
        }
        
        if (bContinue)
        {
          await Task.Delay(1000);
          continue;
        }

        foreach ( var zoneKeyVal in dicLogicToFigures)
        {
          List<TrackPointDTO> tracksInZone = null;
          List<TrackPointDTO> tracksOutZone = null;

          var logic_id = zoneKeyVal.Key;
          box.zone = zoneKeyVal.Value.Select(f => f.location).ToList();

          TrackCounter trackCounter;
          _trackCounters.TryGetValue(logic_id, out trackCounter);

          if (trackCounter != null)
          {
            // Get what we have for current time;
            var inZones = trackCounter.GetInZones();
            
            box.ids = null;
            box.not_in_zone = false;
            tracksInZone = await _tracksService.GetTracksByBox(box);

            // Add figures which were in zone for period in case they cross border.
            var listInZone = tracksInZone.Select(t => t.figure.id).ToList();
            inZones.AddRange(listInZone);

            if (inZones.Count > 0)
            {
              box.not_in_zone = true;
              box.ids = inZones;
              tracksOutZone = await _tracksService.GetTracksByBox(box);              
            }

            bool bChanged = trackCounter.Process(tracksOutZone, tracksInZone);

            if (bChanged)
            {
              await OnUpdateCounterLogic(logic_id);
            }
          }
        }

        await Task.Delay(5000);

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
