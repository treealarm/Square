
using AASubService;
using Dapr.Messaging.PublishSubscribe.Extensions;
using Domain.ServiceInterfaces;
using Google.Api;
using GrpcDaprLib;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using PubSubLib;
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
builder.Services.AddSingleton<ISubService, SubService>();
builder.Services.AddSingleton<IPubService, PubService>();
builder.Services.AddHostedService<SubHostedService>();
builder.Services.AddHostedService<TestPubHostedService>();

var app = builder.Build();
app.Run();
