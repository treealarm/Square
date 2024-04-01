using Google.Protobuf.WellKnownTypes;
using GrpcDaprClientLib;
using LeafletAlarmsGrpc;

namespace GrpcTracksClient
{
  internal class EventAdd
  {
    public static async Task Add()
    {
      using var client = new GrpcUpdater();
      client.Connect(null);
      
      for (int j = 0; j < 100000; j++)
      {
        var events = new EventsProto();       

        for (int i = 0; i < 100; i++)
        {
          var newEv = new EventProto();
          events.Events.Add(newEv);
          newEv.Timestamp = Timestamp.FromDateTime(DateTime.UtcNow);

          newEv.Meta = new EventMetaProto();
          newEv.Meta.EventName = i.ToString() + "_" + j.ToString();
          newEv.Meta.EventPriority = i % 2;
          newEv.ExtraProps.Add(new ProtoObjExtraProperty()
          {
            PropName = "event_name",
            StrVal = "lisa_alert"
          });


          newEv.ExtraProps.Add(new ProtoObjExtraProperty()
          {
            PropName = "event_descr",
            StrVal = "lisa_alert" + j.ToString()
          });

          if (i % 2 == 0)
          {
            newEv.Meta.ObjectId = "64270c097a71c88757377dcf";
          }
          else
          {
            newEv.Meta.ObjectId = "64270c0c7a71c8875738aaa6";
          }          
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
