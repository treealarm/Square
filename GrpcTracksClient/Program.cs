
using Dapr.Client;
using Grpc.Net.Client;
using GrpcTracksClient;
using LeafletAlarmsGrpc;
using static LeafletAlarmsGrpc.TracksGrpcService;




var taskTrack = UpdateTracks.Move();

while (true)
{

  try
  {
    var tasks = new List<Task>
    {
      MoveObject.Move(),
      StateObject.Change()      
    };

    Task.WaitAll(tasks.ToArray());
  }
  catch (Exception ex)
  {
    Console.WriteLine(ex);
    await Task.Delay(1000);
  }
}
