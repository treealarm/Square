using Google.Protobuf.WellKnownTypes;
using IntegrationUtilsLib;
using LeafletAlarmsGrpc;

namespace GrpcTracksClient
{
  internal class EventAdd
  {
    public static EventProto CreateImageEvent(
      LogLevel event_priority,
      string event_name,
      byte[] imageArray,
      string obj_id
      )
    {
      if (imageArray == null)
      {
        return null;
      }
      var newEv = new EventProto();
      newEv.Timestamp = Timestamp.FromDateTime(DateTime.UtcNow);


      newEv.EventPriority = (int)event_priority;
      newEv.EventName = event_name;

      newEv.ExtraProps.Add(new ProtoObjExtraProperty()
      {
        PropName = "license_image",
        StrVal = Convert.ToBase64String(imageArray),
        VisualType = "base64image_fs"
      });

      newEv.ObjectId = obj_id;
     
      return newEv;
    }
    public static async Task Add(CancellationToken token)
    {
      var rnd = new Random();
      var client = Utils.ClientBase.Client;
      
      for (int j = 0; j < 100000; j++)
      {
        if (token.IsCancellationRequested)
        {
          return;
        }
        await Task.Delay(5000);

        var events = new EventsProto();       

        for (int i = 1; i < 99; i++)
        {
          byte[] imageArray = null;
          if (i == 1 || i == 2 || i == 3)
          {
            var fileName = $"files/plate{i}.jpeg";
            imageArray = File.ReadAllBytes(fileName);
          }
            
          var priority = (LogLevel)rnd.Next((int)LogLevel.Trace, (int)LogLevel.None);
          var newEv = CreateImageEvent(
            priority,
            $"plate recognized #{i} sensor {j} event {priority}",
            imageArray,
            i % 2 == 0 ? ObjectIds.ObjectId1: ObjectIds.ObjectId2
            );
          if (newEv == null)
          {
            continue;
          }
          newEv.ExtraProps.Add(new ProtoObjExtraProperty()
          {
            PropName = $"test",
            StrVal = $"{i}"
          });
          events.Events.Add(newEv);
        }
        try
        {
          var result = await client.UpdateEventsAsync(events);
        }
        catch (Exception ex)
        {
          Console.WriteLine(ex.ToString());
          await Task.Delay(5000);
        }
      }

    }
  }
}
