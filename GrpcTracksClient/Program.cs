using GrpcTracksClient;

while (true)
{

  try
  {
    var tasks = new List<Task>
    {
      UpdateSADTracks.Move(),
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
