using GrpcDaprLib;
using LeafletAlarms;
using LeafletAlarms.Grpc.Implementation;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

var startup = new Startup(builder.Configuration);
startup.ConfigureServices(builder.Services);

builder.Services.AddGrpc();

var port = GrpcUpdater.GetGrpcAppPort();

builder.WebHost.ConfigureKestrel(options =>
{
  options.Listen(IPAddress.Any, port, listenOptions =>
  {
    listenOptions.Protocols = HttpProtocols.Http2;
  });
});

var app = builder.Build();
startup.Configure(app, builder.Environment);

app.MapGrpcService<TracksGrpcImp>();
app.Run();
