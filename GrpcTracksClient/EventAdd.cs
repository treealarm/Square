using Google.Protobuf.WellKnownTypes;
using IntegrationUtilsLib;
using LeafletAlarmsGrpc;

namespace GrpcTracksClient
{
  internal class EventAdd
  {
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
          var newEv = new EventProto();
          
          newEv.Timestamp = Timestamp.FromDateTime(DateTime.UtcNow);

         
          newEv.EventPriority = rnd.Next((int)LogLevel.Trace, (int)LogLevel.None);
          newEv.EventName = 
            $"Camera #{i} sensor {j} event {((LogLevel)newEv.EventPriority).ToString()}";

          newEv.ExtraProps.Add(new ProtoObjExtraProperty()
          {
            PropName = $"p{0}",
            StrVal = $"{i}"
          });

          for (var k = 1; k < 3; k++)
          {
            newEv.ExtraProps.Add(new ProtoObjExtraProperty()
            {
              PropName = $"p{k}",
              StrVal = $"{DateTime.Now.Second}"
            });
          }          
          

          if (i == 1 || i == 2 || i == 3)
          {
            newEv.ExtraProps.Add(new ProtoObjExtraProperty()
            {
              PropName = "event_descr",
              StrVal = $"plate recognized"
            });

            var fileName = $"files/plate{i}.jpeg";
            byte[] imageArray = File.ReadAllBytes(fileName);

            newEv.ExtraProps.Add(new ProtoObjExtraProperty()
            {
              PropName = "license_image",
              StrVal = Convert.ToBase64String(imageArray),
              VisualType = "base64image_fs"
            });
          }
          else
          {
            newEv.ExtraProps.Add(new ProtoObjExtraProperty()
            {
              PropName = "event_descr",
              StrVal = $"just description {i}"
            });
          }

          if (i % 2 == 0)
          {
            newEv.ObjectId = ObjectIds.ObjectId1;
          }
          else
          {
            newEv.ObjectId = ObjectIds.ObjectId2;
          }
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

        //await Task.Delay(100);
      }

    }
  }
}
