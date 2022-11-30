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
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace LeafletAlarms.Services
{
  public class LogicProcessorHost : IHostedService, IDisposable
  {
    private Task _timer;
    private CancellationTokenSource _cancellationToken = new CancellationTokenSource();
    private IPubSubService _pubsub;
    private ILogicService _logicService;
    private IGeoService _geoService;
    private readonly IMapService _mapService;
    private readonly ITrackService _tracksService;
    private readonly IUtilService _utilService;
    private object _locker = new object();

    private Dictionary<string, BaseLogicProc> _logicProcs =
      new Dictionary<string, BaseLogicProc>();
    

    private TracksUpdatedEvent _rangeToProcess = new TracksUpdatedEvent();

    public LogicProcessorHost(
      IPubSubService pubsub,
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
    }    

    void OnUpdateTrackPosition(string channel, string message)
    {
      var ev = JsonSerializer.Deserialize<TracksUpdatedEvent>(message);

      if (ev == null)
      {
        return;
      }

      lock(_locker)
      {
        if (_rangeToProcess.ts_start == null ||
          _rangeToProcess.ts_start > ev.ts_start)
        {
          _rangeToProcess.ts_start = ev.ts_start;
        }

        if (_rangeToProcess.ts_end == null ||
          _rangeToProcess.ts_end < ev.ts_end)
        {
          _rangeToProcess.ts_end = ev.ts_end;
        }

        if (_rangeToProcess.id_start == null ||
          _utilService.Compare(_rangeToProcess.id_start, ev.id_start) > 0)
        {
          _rangeToProcess.id_start = ev.id_start;
        }

        if (_rangeToProcess.id_end == null ||
          _utilService.Compare(_rangeToProcess.id_end, ev.id_end) < 0)
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

      await _pubsub.Subscribe("UpdateTrackPosition", OnUpdateTrackPosition);

      _timer = new Task(() => DoWork(), _cancellationToken.Token);
      _timer.Start();
    }

    private async Task OnUpdateLogic(string logic_id)
    {
      BaseLogicProc logicProc = null;

      if (_logicProcs.TryGetValue(logic_id, out logicProc))
      {
        LogicTriggered triggeredVal = new LogicTriggered()
        {
          Text = logicProc.GetUpdatedResult(),
          LogicId = logicProc.LogicId,
          LogicTextObjects = logicProc.TextObjectsIds
        };

        if (logicProc.TextObjectsIds != null && logicProc.TextObjectsIds.Count > 0)
        {
          var updatedProps = new List<ObjPropsDTO>();

          foreach (var textObjId in logicProc.TextObjectsIds)
          {
            var updatedObj = new ObjPropsDTO()
            {
              id = textObjId,
              extra_props = new List<ObjExtraPropertyDTO>()
              {
                new ObjExtraPropertyDTO()
                {
                  prop_name = "text",
                  str_val = triggeredVal.Text
                }
              }
            };

            updatedProps.Add(updatedObj);
          }

          await _mapService.UpdatePropNotDeleteAsync(updatedProps);
        }

        await _pubsub.Publish("LogicTriggered", JsonSerializer.Serialize(triggeredVal));
      }
    }

    private async void DoWork()
    {
      var logics = await _logicService.GetListAsync(null, true, 1000);

      BoxTrackDTO box = new BoxTrackDTO();

      foreach (var logic in logics)
      {
        BaseLogicProc logicProc = null;

        if (logic.logic == "counter")
        {
          logicProc = new TrackCounter(logic);
          _logicProcs[logic.id] = logicProc;
        }

        if (logic.logic == "from-to")
        {
          logicProc = new FromToTrigger(logic);
          _logicProcs[logic.id] = logicProc;
        }

        await logicProc?.InitFromDb(_geoService);
        await OnUpdateLogic(logic.id);
      }


      while (!_cancellationToken.IsCancellationRequested)
      {
        bool bContinue = false;

        lock (_locker)
        {
          if (_rangeToProcess.ts_start == null ||
            _rangeToProcess.ts_end == null)
          {
            bContinue = true;
          }

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

        foreach (var logicProc in _logicProcs)
        {      
          var curLogicProc = logicProc.Value;

          var bChanged = await curLogicProc.ProcessTracks(
            _tracksService,
            box.time_start,
            box.time_end
            );

          if (bChanged)
          {
            await OnUpdateLogic(curLogicProc.LogicId);
          }
        }

        await Task.Delay(5000);
      }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
      await _pubsub.Unsubscribe("UpdateTrackPosition", OnUpdateTrackPosition);
      _cancellationToken.Cancel();
      _timer?.Wait();
    }
  }
}
