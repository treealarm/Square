
using AASubService;
using AASubService.Services;
using Dapr.Messaging.PublishSubscribe.Extensions;
using Domain;
using GrpcDaprLib;
using IntegrationUtilsLib;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using PubSubLib;


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
builder.Services.AddSingleton<ISquareIntegration>(SquareIntegration.Default);

//builder.Services.AddSingleton<ISubServiceLu, SubServiceLu>();
//builder.Services.AddSingleton<IPubServiceLu, PubServiceLu>();
//builder.Services.AddHostedService<SubHostedService>();
//builder.Services.AddHostedService<TestPubHostedService>();

builder.Services.AddSingleton<CameraManager>();
builder.Services.AddSingleton<CameraEventServiceManager>();
builder.Services.AddSingleton<ICameraManager>(sp => sp.GetRequiredService<CameraManager>());
builder.Services.AddSingleton<IObjectActions>(sp => sp.GetRequiredService<CameraManager>());

builder.Services.AddHostedService<CamerasHostedService>();

var app = builder.Build();

app.MapGrpcService<ActionsServiceImpl>();

app.Run();
