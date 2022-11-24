using DbLayer.Services;
using Domain.ServiceInterfaces;
using Domain.StateWebSock;
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
    private List<TrackPointDTO> _listOfNewTracks = new List<TrackPointDTO>();
    private object _locker = new object();
    public LogicProcessorHost(
      PubSubService pubsub,
      ILogicProcessorService logicProcessorService,
      ILogicService logicService,
      IGeoService geoService
    )
    {
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
      await _logicProcessorService.Drop();
      var logics = await _logicService.GetListAsync(null, true, 1000);

      foreach (var logic in logics)
      {
        var geoFigs = await
            _geoService.GetGeoObjectsAsync(logic.figs.Select(f => f.id).ToList());

        foreach (var fig in geoFigs)
        {
          await _logicProcessorService.Insert(fig.Value, logic.id);
        }
      }

      _pubsub.Subscribe("UpdateTrackPosition", OnUpdateTrackPosition);

      _timer = new Task(() => DoWork(), _cancellationToken.Token);
      _timer.Start();
    }

    private async void DoWork()
    {
      while (!_cancellationToken.IsCancellationRequested)
      {
        List<TrackPointDTO> listOfNewTracks;

        lock (_locker)
        {
          listOfNewTracks = _listOfNewTracks;
          _listOfNewTracks = new List<TrackPointDTO>();
        }

        foreach (var trackPoint in listOfNewTracks)
        {
          var logics = await _logicProcessorService.GetLogicByFigure(trackPoint.figure);

          if (logics == null || logics.Count == 0)
          {
            continue;
          }

          int test = 0;
        }

        await Task.Delay(1000);
        continue;
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
