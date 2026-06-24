
using AASubService;
using AASubService.Services;
using Dapr.Messaging.PublishSubscribe.Extensions;
using Domain;
using GrpcDaprLib;
using IntegrationUtilsLib;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using PubSubLib;
using SquareIntegrationClient;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();

var grpc_port = GrpcBaseUpdater.GetAppPort("APP_PORT", 20008);

builder.WebHost.ConfigureKestrel(options =>
{
  options.Listen(System.Net.IPAddress.Any, grpc_port, listenOptions =>
  {
    listenOptions.Protocols = HttpProtocols.Http2;
  });

  options.Listen(System.Net.IPAddress.Any, 5224, listenOptions =>
  {
    listenOptions.Protocols = HttpProtocols.Http1;
  });
});

builder.Services.AddDaprPubSubClient();

builder.Services.AddSingleton<ISubService, SubService>();
// Factory form, not the already-evaluated instance: SquareIntegration.Default's getter can throw
// (missing env var, sidecar not ready yet) and is designed to be retried on the next access
// instead of caching the failure — passing an instance here would evaluate it once, synchronously,
// at this exact line, crashing startup outright with no retry opportunity at all.
builder.Services.AddSingleton<ISquareIntegration>(_ => SquareIntegration.Default);

builder.Services.AddSingleton<CameraManager>();
builder.Services.AddSingleton<CameraEventServiceManager>();
builder.Services.AddSingleton<ICameraManager>(sp => sp.GetRequiredService<CameraManager>());
builder.Services.AddSingleton<IObjectActions>(sp => sp.GetRequiredService<CameraManager>());

builder.Services.AddHostedService<CamerasHostedService>();

var app = builder.Build();

app.MapGrpcService<ActionsServiceImpl>();

app.Run();
