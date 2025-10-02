
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

_ = Task.Run(async () =>
{
  return;
  try
  {
    //http://172.16.254.136/onvif/services
    //var camera = await Camera.CreateAsync("172.16.254.103", 80, "admin", "en123456");
    var camera = Camera.Create("172.16.254.136", 80, "root", "root");
    var service = await camera.GetEventService();

    if (service == null)
    {
      Console.WriteLine("Error.");
      return;
    }

    service.OnEventReceived += msgs =>
    {
      foreach (var msg in msgs)
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
      }      
    };

    await service.StartReceivingAsync();
  }
  catch (Exception ex)
  {
    Console.WriteLine($"Error: {ex.Message}");
  }
});

app.Run();
