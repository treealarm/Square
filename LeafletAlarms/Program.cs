using GrpcDaprLib;
using LeafletAlarms;
using LeafletAlarms.Authentication;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

var startup = new Startup(builder.Configuration);
startup.ConfigureServices(builder.Services);

// Приёмный gRPC-слой интеграции вынесен в отдельный процесс IntegrationHost
// (см. docs/integration-host-extraction.md). Здесь — только фронт-API (REST/WebSocket/SPA).
var http_port = GrpcBaseUpdater.GetAppPort("HTTP_PORT", 8000);

builder.WebHost.ConfigureKestrel(options =>
{
  options.Listen(IPAddress.Any, http_port);
});

var app = builder.Build();
app.UseMiddleware<RealmMiddleware>();

startup.Configure(app, builder.Environment);

app.Run();
