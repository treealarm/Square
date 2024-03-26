using GrpcTracksClient;

var taskTrack = UpdateByGRPC.Move();



while (true)
{

  try
  {
    var tasks = new List<Task>
    {
      MoveObject.Move(),
      StateObject.Change(),
      EventAdd.Add()
    };

    Task.WaitAll(tasks.ToArray());
  }
  catch (Exception ex)
  {
    Logger.LogException(ex);
    await Task.Delay(1000);
  }
}
