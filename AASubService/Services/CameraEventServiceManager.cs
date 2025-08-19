using AASubService.Events;
using EventServiceReference1;
using IntegrationUtilsLib;
using LeafletAlarmsGrpc;
using OnvifLib;
using System.Collections.Concurrent;

namespace AASubService.Services
{
  public class CameraEventServiceManager
  {
    private readonly ConcurrentDictionary<string, EventService1> _activeServices = new();

    public async Task AddCameraAsync(string cameraId, Camera camera)
    {
      if (_activeServices.ContainsKey(cameraId))
        return;

      try
      {
        var service = await camera.GetEventService();
        if (service == null)
        {
          Console.WriteLine($"[{cameraId}] Event service is null");
          return;
        }

        service.OnEventReceived += msgs =>
        {
          _ = HandleEventAsync(msgs, camera, cameraId);
        };


        await service.StartReceiving();

        if (!_activeServices.TryAdd(cameraId, service))
        {
          await service.StopReceivingAsync();
          service.Dispose();
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine($"[{cameraId}] Failed to start event service: {ex.Message}");
      }
    }


    public async Task RemoveCameraAsync(string cameraId)
    {
      if (_activeServices.TryRemove(cameraId, out var service))
      {
        Console.WriteLine($"[{cameraId}] Stopping event service...");
        await service.StopReceivingAsync();
        service.Dispose();
      }
    }

    public async Task StopAll()
    {
      foreach (var (cameraId, service) in _activeServices)
      {
        await service.StopReceivingAsync();
        service.Dispose();
      }

      _activeServices.Clear();
    }

    async Task HandleEventAsync(NotificationMessageHolderType[] msgs, Camera camera, string cameraId)
    {
      try
      {
        var client = Utils.ClientBase.Client;

        Console.WriteLine($"[{cameraId}] Event received");

        if (client ==  null)
        {
          return;
        }

        // Получение изображения
        var media = await camera.GetMediaService();
        ImageResult? imageResult = null;

        if (media != null)
        {
          imageResult = await media.GetImage(); // byte[]
        }

        UploadFileProto? protoUploadFile = null;

        if (imageResult != null)
        {
          // Upload image only once for all events.
          protoUploadFile = new UploadFileProto()
          {
            MainFolder = "events",
            Path = DateTime.UtcNow.ToString("yyyyMMdd"),
            FileName = Guid.NewGuid().ToString()
          };

          protoUploadFile.FileData = Google.Protobuf.ByteString.CopyFrom(imageResult.Data);
          await client.UploadFileAsync(protoUploadFile);
        }        

        var events = new EventsProto();

        foreach (var msg in msgs)
        {
          var eventProto = OnvifEventMapper.Map(msg, cameraId);

          if (protoUploadFile != null && protoUploadFile.FileData?.Length > 0)
          {
            eventProto.ExtraProps.Add(new ProtoObjExtraProperty
            {
              PropName = "snapshot",
              StrVal = Path.Combine(
                protoUploadFile.MainFolder, 
                protoUploadFile.Path, 
                protoUploadFile.FileName),
              VisualType = "image_fs"
            });
          }
          events.Events.Add(eventProto);
        }
        await client!.UpdateEventsAsync(events);
      }
      catch (Exception ex)
      {
        Console.WriteLine($"[{cameraId}] Error in event forwarding: {ex.Message}");
      }
    }
  }

}
