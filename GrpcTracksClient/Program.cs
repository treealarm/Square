using GrpcTracksClient;

var cancellationTokenSource = new CancellationTokenSource();
var cancellationToken = cancellationTokenSource.Token;

var tasks = new List<Task>
{
    RunTaskWithRetry(() => UpdateSADTracks.Move(), "UpdateSADTracks.Move", cancellationToken),
    RunTaskWithRetry(() => MoveObject.MoveCars(cancellationToken), "MoveObject.MoveCars", cancellationToken),
    RunTaskWithRetry(() => MoveObject.MovePolygons(), "MoveObject.MovePolygons", cancellationToken),
    RunTaskWithRetry(() => StateObject.Change(), "StateObject.Change", cancellationToken),
    RunTaskWithRetry(() => EventAdd.Add(), "EventAdd.Add", cancellationToken),
    RunTaskWithRetry(() => DiagramUpdater.UploadDiagramsAsync(), "DiagramUpdater.UploadDiagramsAsync", cancellationToken)
};

// Ожидание завершения всех задач (или их отмены)
await Task.WhenAll(tasks);

async Task RunTaskWithRetry(Func<Task> taskFunc, string taskName, CancellationToken token)
{
  while (!token.IsCancellationRequested)
  {
    try
    {
      await taskFunc();
    }
    catch (Exception ex)
    {
      Logger.LogException(ex);
      await Task.Delay(1000, token); // Задержка перед повторной попыткой
    }
  }
}

