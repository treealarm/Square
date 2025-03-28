
using AASubService;
using AASubService.Services;
using Dapr.Messaging.PublishSubscribe.Extensions;
using GrpcDaprLib;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();

var grpc_port = GrpcUpdater.GetAppPort();

builder.WebHost.ConfigureKestrel(options =>
{
  options.Listen(IPAddress.Any, grpc_port, listenOptions =>
  {
    listenOptions.Protocols = HttpProtocols.Http2;
  });
});

builder.Services.AddDaprPubSubClient();
builder.Services.AddSingleton<ISubServiceLu, SubServiceLu>();
builder.Services.AddSingleton<IPubServiceLu, PubServiceLu>();
builder.Services.AddHostedService<SubHostedService>();
builder.Services.AddHostedService<TestPubHostedService>();

builder.Services.AddSingleton<CameraService>();
builder.Services.AddSingleton<IObjectActions>(provider => provider.GetRequiredService<CameraService>());


var app = builder.Build();

app.MapGrpcService<ActionsServiceImpl>();

Camera cam = new Camera("http://192.168.1.150:8899/onvif/device_service", "danil", "$Power321");
var servicesTask = cam.Init();
servicesTask?.ContinueWith(task =>
{
  if (task.IsFaulted)
  {
    Console.WriteLine($"Ошибка: {task.Exception?.GetBaseException().Message}");
    return;
  }

  if (task.IsCompletedSuccessfully)
  {
  }
});
app.Run();
