
using Domain;
using IntegrationUtilsLib;
using LeafletAlarmsGrpc;
using System.Collections.Concurrent;

namespace AASubService
{
  internal class CamerasHostedService : IHostedService, IAsyncDisposable
  {

    private Task? _timer;
    private CancellationTokenSource _cancellationToken = new CancellationTokenSource();
    private IntegrationSyncFull _sync;
    private const string CamStr = "cam";
    private ConcurrentDictionary<string, Camera> _cameras = new ConcurrentDictionary<string, Camera>();
    public CamerasHostedService(ISubService sub)
    {
      var type_to_props = new Dictionary<string, List<ProtoObjExtraProperty>?>();
      type_to_props.Add(IntegrationSync.MainStr, null);

      type_to_props.Add(CamStr, new List<ProtoObjExtraProperty>());

      type_to_props[CamStr]!.Add(new ProtoObjExtraProperty()
      {
        PropName = "ip",
        StrVal = "127.0.0.1",
        VisualType = ""
      });
      type_to_props[CamStr]!.Add(new ProtoObjExtraProperty()
      {
        PropName = "port",
        StrVal = "80",
        VisualType = ""
      });
      type_to_props[CamStr]!.Add(new ProtoObjExtraProperty()
      {
        PropName = "user",
        StrVal = "root",
        VisualType = ""
      });
      type_to_props[CamStr]!.Add(new ProtoObjExtraProperty()
      {
        PropName = "password",
        StrVal = "root",
        VisualType = VisualTypes.Password
      });
      _sync =  new IntegrationSyncFull(sub, type_to_props, _cancellationToken.Token);  
    }

    async Task IHostedService.StartAsync(CancellationToken cancellationToken)
    {
      await Task.Delay(1);
      // Start timer after processing initial states.
      _timer = new Task(() => DoWork(), _cancellationToken.Token);
      _timer.Start();      
    }

    async Task IHostedService.StopAsync(CancellationToken cancellationToken)
    {
      _cancellationToken.Cancel();
      _timer?.Wait();

      await Task.Delay(1);
    }

    public async ValueTask DisposeAsync()
    {
      _timer?.Dispose();
      await _sync.DisposeAsync();
    }

    private async Task InitCameras()
    {
      _cameras.Clear();

      var propsById = _sync.ObjProps.Values
        .Where(prop => prop != null)
        .ToDictionary(
            prop => prop!.Id,
            prop => prop?.Properties.ToDictionary(p => p.PropName, p => p)
        );

      foreach (var prop in propsById)
      {
        if (prop.Value == null)
        { continue; }

        var ipProp = prop.Value["ip"];
        var portProp = prop.Value["port"];
        var userProp = prop.Value["user"];
        var passwordProp = prop.Value["password"];

        var cam = await Camera.CreateAsync(
          ipProp.StrVal,
          Int32.Parse(portProp.StrVal),
          userProp.StrVal,
          passwordProp.StrVal
          );
        if (cam == null)
        { continue; }
        _cameras[prop.Key] = cam;
      }
    }
    private async void DoWork()
    {
      await Task.Delay(10000);
      await _sync.InitAll();

      var hierarchy = new Dictionary<string, IEnumerable<string>>
      {
        [IntegrationSync.MainStr] = new[] { CamStr },
        //["cam"] = new[] { "lens" },
        //["sensor"] = Array.Empty<string>(),
        //["lens"] = Array.Empty<string>()
      };

      await _sync.InitChildrenTypes(hierarchy);

      bool bInitRequest = true;

      _sync.OnConfigurationChanged += () =>
      {
        bInitRequest = true;
      };

      while (!_cancellationToken.IsCancellationRequested)
      {
        if (bInitRequest)
        {
          bInitRequest = false;
          await InitCameras();
        }

        foreach (var cam in _cameras)
        {
          var media  = await cam.Value.GetMediaService();
          if (media == null)
          {
            continue;
          }
          var data = await media.GetImage();
          if (data != null)
          {
            //var filePath = "M:\\snapshot.jpg";
            //await File.WriteAllBytesAsync(filePath, data);
          }
        }
        await Task.Delay(5000);        
      }
    }
  }
}
