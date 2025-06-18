using Google.Protobuf.WellKnownTypes;
using IntegrationUtilsLib;
using LeafletAlarmsGrpc;

namespace GrpcTracksClient
{
  internal class StateObject
  {
    public static async Task Change()
    {
      var client = Utils.ClientBase;
      var states = new ProtoObjectStates();

      var state = new ProtoObjectState();
      states.States.Add(state);

   
      
      state.Timestamp = Timestamp.FromDateTime(DateTime.UtcNow);

      for (int i = 0; i < 100; i++)
      {
        state.Id = ObjectIds.ObjectId1;

        if (DateTime.UtcNow.Microsecond % 2 == 0)
        {
          state.Id = ObjectIds.ObjectId2;
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

        await client.Client.UpdateStatesAsync(states);
        await Task.Delay(1000);
      }      
    }
  }
}
