using Google.Protobuf.WellKnownTypes;
using GrpcDaprLib;
using LeafletAlarmsGrpc;

namespace GrpcTracksClient
{
  internal class StateObject
  {
    public static async Task Change()
    {
      using var client = new GrpcUpdater();
      client.Connect(null);
      var states = new ProtoObjectStates();

      var state = new ProtoObjectState();
      states.States.Add(state);

   
      
      state.Timestamp = Timestamp.FromDateTime(DateTime.UtcNow);

      for (int i = 0; i < 100; i++)
      {
        state.Id = "64270c097a71c88757377dcf";

        if (DateTime.UtcNow.Microsecond % 2 == 0)
        {
          state.Id = "64270c0c7a71c8875738aaa6";
        }

        state.States.Clear();

        if (i % 2 == 0)
        {
          state.States.Add("ALARM");
        }
        else
        {
          state.States.Add("NORM");
        }

        await client.SendStates(states);
        await Task.Delay(1000);
      }      
    }
  }
}
