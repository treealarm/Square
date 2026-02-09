using GrpcDaprLib;
using GrpcTracksClient;
using GrpcTracksClient.Services;
using IntegrationUtilsLib;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using System.Net;


var builder = WebApplication.CreateBuilder(args);

// Добавляем gRPC в DI-контейнер
builder.Services.AddGrpc();

var grpc_port = GrpcBaseUpdater.GetAppPort("APP_PORT", 5001);

builder.WebHost.ConfigureKestrel(options =>
{
  options.Listen(IPAddress.Any, grpc_port, listenOptions =>
  {
    listenOptions.Protocols = HttpProtocols.Http2;
  });
});


builder.Services.AddSingleton<MoveObjectService>();
builder.Services.AddSingleton<IMoveObjectService>(provider => provider.GetRequiredService<MoveObjectService>());
builder.Services.AddSingleton<IObjectActions>(provider => provider.GetRequiredService<MoveObjectService>());
builder.Services.AddHostedService<HostedServiceImp>();

// Создаём приложение
var app = builder.Build();

// Регистрируем gRPC-сервис
app.MapGrpcService<ActionsServiceImpl>();



var cancellationTokenSource = new CancellationTokenSource();
var cancellationToken = cancellationTokenSource.Token;

var appTask = app.RunAsync();

var tasks = new List<Task>
{
    Utils.RunTaskWithRetry(() => UpdateSADTracks.Move(), "UpdateSADTracks.Move", cancellationToken),
    Utils.RunTaskWithRetry(() => StateObject.Change(), "StateObject.Change", cancellationToken),
    Utils.RunTaskWithRetry(() => EventAdd.Add(cancellationToken), "EventAdd.Add", cancellationToken),
    Utils.RunTaskWithRetry(() => DiagramUpdater.UploadDiagramsAsync(), "DiagramUpdater.UploadDiagramsAsync", cancellationToken)
};

// Ожидание завершения всех задач (или их отмены)
await Task.WhenAll(tasks);

await appTask;


