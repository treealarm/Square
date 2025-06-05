
using AASubService;
using Dapr.Messaging.PublishSubscribe.Extensions;
using Domain;
using GrpcDaprLib;
using IntegrationUtilsLib;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using PubSubLib;
using System.Text;
using System.Xml;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();

var grpc_port = GrpcBaseUpdater.GetAppPort();

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

builder.Services.AddSingleton<ISubServiceLu, SubServiceLu>();
builder.Services.AddSingleton<IPubServiceLu, PubServiceLu>();
builder.Services.AddHostedService<SubHostedService>();
builder.Services.AddHostedService<TestPubHostedService>();

builder.Services.AddSingleton<CameraManager>();
builder.Services.AddSingleton<ICameraManager>(sp => sp.GetRequiredService<CameraManager>());
builder.Services.AddSingleton<IObjectActions>(sp => sp.GetRequiredService<CameraManager>());

builder.Services.AddHostedService<CamerasHostedService>();

var app = builder.Build();

app.MapGrpcService<ActionsServiceImpl>();
_ = Task.Run(async () =>
{
  try
  {
    var camera = await Camera.CreateAsync("172.16.254.103", 80, "admin", "en123456");
    var service = await camera.GetEventService();

    if (service == null)
    {
      Console.WriteLine("Error.");
      return;
    }

    service.OnEventReceived += msg =>
    {
      Console.WriteLine("=== Notification Received ===");
      Console.WriteLine("Topic:");
      foreach (var node in msg.Topic.Any)
      {
        Console.WriteLine(node.OuterXml);
      }
      Console.WriteLine("Message:");
      Console.WriteLine(msg.Message.OuterXml);
      Console.WriteLine("=============================");
    };

    await service.StartReceiving();
  }
  catch (Exception ex)
  {
    Console.WriteLine($"Error: {ex.Message}");
  }
});

app.Run();
