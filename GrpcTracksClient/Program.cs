using GrpcDaprLib;
using GrpcTracksClient;
using Microsoft.AspNetCore.Server.Kestrel.Core;


var builder = WebApplication.CreateBuilder(args);

// Добавляем gRPC в DI-контейнер
builder.Services.AddGrpc();

var port = GrpcUpdater.GetGrpcAppPort();

builder.WebHost.ConfigureKestrel(options =>
{
  options.ListenLocalhost(port, listenOptions =>
  {
    listenOptions.Protocols = HttpProtocols.Http2;
  });
});

// Создаём приложение
var app = builder.Build();


// Регистрируем gRPC-сервис
app.MapGrpcService<ActionsServiceImpl>();

var cancellationTokenSource = new CancellationTokenSource();
var cancellationToken = cancellationTokenSource.Token;

var appTask = app.RunAsync();

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

await appTask;


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

