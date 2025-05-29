
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
    Console.WriteLine($"Ошибка: {ex.Message}");
  }
});

app.MapPost("/onvif/events/", async (HttpRequest req) =>
{
  using var memoryStream = new MemoryStream();

  try
  {
    req.EnableBuffering();

    var buffer = new byte[1024]; 
    
    int bytesRead;

    do
    {
      bytesRead = await req.Body.ReadAsync(buffer, 0, buffer.Length);
      memoryStream.Write(buffer, 0, bytesRead);
    } while (bytesRead > 0);

    var body = Encoding.UTF8.GetString(memoryStream.ToArray());
    if (!string.IsNullOrEmpty(body))
      Camera.ParseEvent(body);
  }
  catch (Exception ex) 
  {
    Console.WriteLine(ex.ToString()); 
  }
  string body1 = Encoding.UTF8.GetString(memoryStream.ToArray());

  return Results.Ok(); // Камера ждет HTTP 200
});

app.Run();
