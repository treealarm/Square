using Dapr.Messaging.PublishSubscribe.Extensions;
using Domain;
using GrpcDaprLib;
using IntegrationHost;
using IntegrationServerLib;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using PubSubLib;
using System.Net;

// IntegrationHost — приёмный gRPC-слой интеграции (продьюсеры пушат сюда дерево объектов,
// свойства, состояния, события, команды-результаты). Пишет в ту же PostgreSQL (по DB_REALM_NAME)
// и публикует изменения в pub-sub; фронт-API (LeafletAlarms) подхватывает их подпиской.
// См. docs/integration-host-extraction.md.

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc(options =>
{
  options.Interceptors.Add<GrpcContextInterceptor>();
});

var grpc_port = GrpcBaseUpdater.GetAppPort("APP_PORT", 5000);

builder.WebHost.ConfigureKestrel(options =>
{
  options.Listen(IPAddress.Any, grpc_port, listenOptions =>
  {
    listenOptions.Protocols = HttpProtocols.Http2;
  });
});

builder.Services.Configure<RoutingSettings>(builder.Configuration.GetSection("RoutingSettings"));

builder.Services.AddDaprPubSubClient();
builder.Services.AddSingleton<ISubService, SubService>();
builder.Services.AddSingleton<IPubService, PubService>();

// Переиспользуем штатную регистрацию слоя данных и изменений (DbContext по DB_REALM_NAME,
// write-сервисы). Миграции остаются за LeafletAlarms — здесь их не запускаем.
DbLayer.ServicesConfigurator.ConfigureServices(builder.Services, builder.Configuration);
DataChangeLayer.ServicesConfigurator.ConfigureServices(builder.Services);

builder.Services.AddScoped<GRPCServiceProxy>();
builder.Services.AddSingleton<FileSystemService>();
builder.Services.AddSingleton<GrpcRequestContextProvider>();

var app = builder.Build();

app.MapGrpcService<TracksGrpcImp>();
app.MapGrpcService<IntegroGrpcImp>();

app.Run();
