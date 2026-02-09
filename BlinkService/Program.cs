using Dapr.Messaging.PublishSubscribe.Extensions;
using Domain;
using GrpcDaprLib;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using PubSubLib;
using System.Net;

namespace BlinkService
{
  internal class Program
  {
    public static async Task Main(string[] args)
    {
      Console.WriteLine("Hello, Blink service!");

      var builder = WebApplication.CreateBuilder(args);

      // ---------- Logging ----------
      builder.Logging.ClearProviders();
      builder.Logging.AddConsole();
      builder.Logging.SetMinimumLevel(LogLevel.Warning);

      var services = builder.Services;
      var config = builder.Configuration;

      builder.Services.AddGrpc();
      // ---------- Dapr ----------
      services.AddDaprPubSubClient();

      services.AddActors(options =>
      {
        options.Actors.RegisterActor<AlarmActor>();
      });

      // ---------- App services ----------
      services.AddSingleton<ISubService, SubService>();
      services.AddSingleton<IPubService, PubService>();

      DbLayer.ServicesConfigurator.ConfigureServices(services, config);
      DataChangeLayer.ServicesConfigurator.ConfigureServices(services);

      // ---------- Background workers ----------
      services.AddHostedService<HierarhyStateService>();

      //var grpc_port = GrpcBaseUpdater.GetAppPort("APP_PORT", 20009);
      var http_port = GrpcBaseUpdater.GetAppPort("HTTP_PORT", 20009);

      builder.WebHost.ConfigureKestrel(options =>
      {
        // this configuration is the same as:
        //- ASPNETCORE_URLS=http://+:8000;http://+:${GRPC_MAIN_PORT}
        //- Kestrel__Endpoints__gRPC__Url=http://*:${GRPC_MAIN_PORT}
        //- Kestrel__Endpoints__gRPC__Protocols=Http2
        //- Kestrel__Endpoints__Http__Url=http://*:8000
        //options.Listen(IPAddress.Any, grpc_port, listenOptions =>
        //{
        //  listenOptions.Protocols = HttpProtocols.Http2;
        //});
        options.Listen(IPAddress.Any, http_port);
      });

      var app = builder.Build();
      app.UseRouting();
      // ---------- Dapr endpoints ----------
      app.MapActorsHandlers();

      // Простой тестовый endpoint
      app.MapGet("/ping", () => Results.Ok(new { status = "ok", time = DateTime.UtcNow }));

      // (если будут pub/sub контроллеры — можно добавить)
      // app.MapSubscribeHandler();

      await app.RunAsync();
    }
  }
}
