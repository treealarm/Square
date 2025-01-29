using Google.Protobuf.WellKnownTypes;
using GrpcDaprLib;
using LeafletAlarmsGrpc;
using Microsoft.Extensions.Logging;

namespace GrpcTracksClient
{
  internal class EventAdd
  {
    public static async Task Add()
    {
      var rnd = new Random();
      var client = Utils.Client;
      
      for (int j = 0; j < 100000; j++)
      {
        await Task.Delay(1);
        var events = new EventsProto();       

        for (int i = 0; i < 100; i++)
        {
          var newEv = new EventProto();
          
          newEv.Timestamp = Timestamp.FromDateTime(DateTime.UtcNow);

          newEv.Meta = new EventMetaProto();
          
          newEv.EventPriority = rnd.Next((int)LogLevel.Trace, (int)LogLevel.None);
          newEv.EventName = 
            $"Camera #{i} sensor {j} event {((LogLevel)newEv.EventPriority).ToString()}";

          newEv.Meta.ExtraProps.Add(new ProtoObjExtraProperty()
          {
            PropName = "indexed_prop",
            StrVal = $"you can search me {i}"
          });
          

          if (i <= 1)
          {
            newEv.Meta.NotIndexedProps.Add(new ProtoObjExtraProperty()
            {
              PropName = "event_descr",
              StrVal = $"plate recognized"
            });

            var fileName = $"files/plate{i}.jpeg";
            byte[] imageArray = File.ReadAllBytes(fileName);

            newEv.Meta.NotIndexedProps.Add(new ProtoObjExtraProperty()
            {
              PropName = "license_image",
              StrVal = Convert.ToBase64String(imageArray),
              VisualType = "base64image_fs"
            });
          }
          else
          {
            newEv.Meta.NotIndexedProps.Add(new ProtoObjExtraProperty()
            {
              PropName = "event_descr",
              StrVal = $"you can't search me"
            });
          }

          if (i % 2 == 0)
          {
            newEv.ObjectId = "64270c097a71c88757377dcf";
          }
          else
          {
            newEv.ObjectId = "64270c0c7a71c8875738aaa6";
          }
          events.Events.Add(newEv);
        }
        try
        {
          var result = await client.AddEvents(events);
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
