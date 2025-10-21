using GrpcDaprLib;
using LeafletAlarms;
using LeafletAlarms.Authentication;
using LeafletAlarms.Grpc.Implementation;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

var startup = new Startup(builder.Configuration);
startup.ConfigureServices(builder.Services);

builder.Services.AddGrpc(options =>
{
  options.Interceptors.Add<GrpcContextInterceptor>();
});

var grpc_port = GrpcBaseUpdater.GetAppPort("APP_PORT", 5000);
var http_port = GrpcBaseUpdater.GetAppPort("HTTP_PORT", 8000);

builder.WebHost.ConfigureKestrel(options =>
{
  // this configuration is the same as:
//- ASPNETCORE_URLS=http://+:8000;http://+:${GRPC_MAIN_PORT}
//- Kestrel__Endpoints__gRPC__Url=http://*:${GRPC_MAIN_PORT}
//- Kestrel__Endpoints__gRPC__Protocols=Http2
//- Kestrel__Endpoints__Http__Url=http://*:8000
  options.Listen(IPAddress.Any, grpc_port, listenOptions =>
  {
    listenOptions.Protocols = HttpProtocols.Http2;
  });
  options.Listen(IPAddress.Any, http_port);
});

var app = builder.Build();
app.UseMiddleware<RealmMiddleware>();

startup.Configure(app, builder.Environment);

app.MapGrpcService<TracksGrpcImp>();
app.MapGrpcService<IntegroGrpcImp>();
app.Run();
