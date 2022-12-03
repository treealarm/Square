using Domain;
using Domain.GeoDTO;
using Domain.Logic;
using Domain.NonDto;
using Domain.ServiceInterfaces;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace LogicMicroService
{
  public class LogicProcessorHost : BackgroundService
  {
    private readonly ILogger<LogicProcessorHost> _logger;

    private CancellationTokenSource _cancellationToken = new CancellationTokenSource();
    private IPubSubService _pubsub;
    private ILogicService _logicService;
    private IGeoService _geoService;
    private readonly IMapService _mapService;
    private readonly ITrackService _tracksService;
    private object _locker = new object();

    private Dictionary<string, BaseLogicProc> _logicProcs =
      new Dictionary<string, BaseLogicProc>();

    private HashSet<string> _logicIdsToUpdate = new HashSet<string>();


    private List<TracksUpdatedEvent> _rangeToProcess = new List<TracksUpdatedEvent>();

    public LogicProcessorHost(
      IPubSubService pubsub,
      ILogicService logicService,
      IGeoService geoService,
      ITrackService tracksService,
      IMapService mapService,
      ILogger<LogicProcessorHost> logger
    )
    {
      _logger = logger;
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

      lock (_locker)
      {
        _rangeToProcess.Add(ev);
      }
    }

    void OnUpdateLogicProc(string channel, string message)
    {
      var ids = JsonSerializer.Deserialize<List<string>>(message);

      if (ids == null)
      {
        return;
      }

      lock(_locker)
      {
        foreach (var id in ids)
        {
          _logicIdsToUpdate.Add(id);
        }
      }      
    }

    public async override Task StartAsync(CancellationToken cancellationToken)
    {
      await _pubsub.Subscribe("UpdateTrackPosition", OnUpdateTrackPosition);
      await _pubsub.Subscribe("UpdateLogicProc", OnUpdateLogicProc); 

      await base.StartAsync(cancellationToken);
    }

    private async Task OnUpdateLogic(string logic_id)
    {
      BaseLogicProc logicProc;

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

        _pubsub.PublishNoWait("LogicTriggered", JsonSerializer.Serialize(triggeredVal));
      }
    }

    private async Task  DoUpdateLogicProc(List<StaticLogicDTO> logics)
    {
      foreach (var logic in logics)
      {
        _logicProcs.Remove(logic.id);

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

        await logicProc?.InitFromDb(_geoService, _mapService);
        await OnUpdateLogic(logic.id);
      }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
      var logics = await _logicService.GetListAsync(null, true, 1000);

      BoxTrackDTO box = new BoxTrackDTO();

      await DoUpdateLogicProc(logics);

      while (!_cancellationToken.IsCancellationRequested)
      {
        bool bContinue = false;
        List<string> logicToUpdate = null;

        lock (_locker)
        {
          if (_rangeToProcess.Count == 0)
          {
            bContinue = true;
          }
          else
          {
            box.time_start = _rangeToProcess.MinBy(e => e.ts_start).ts_start;
            box.time_end = _rangeToProcess.MaxBy(e => e.ts_end).ts_end;
          }          

          _rangeToProcess.Clear();

          if (_logicIdsToUpdate.Count > 0)
          {
            logicToUpdate = _logicIdsToUpdate.ToList();
            _logicIdsToUpdate.Clear();
          }
        }

        if (logicToUpdate != null && logicToUpdate.Count > 0)
        {          
          logics = await _logicService.GetListByIdsAsync(logicToUpdate);

          var logicToDelete = logicToUpdate
            .Where(id => logics.Find(l => l.id == id) == null)
            .ToList();

          foreach (var del in logicToDelete)
          {
            _logicProcs.Remove(del);
          }

          await DoUpdateLogicProc(logics);
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

    public async override Task StopAsync(CancellationToken cancellationToken)
    {
      await _pubsub.Unsubscribe("UpdateTrackPosition", OnUpdateTrackPosition);
      await base.StopAsync(cancellationToken);
    }
  }
}