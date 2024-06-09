using DbLayer.Services;
using Domain.OptionsModels;
using Domain.ServiceInterfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using PubSubLib;

namespace BlinkService
{
  internal class Program
  {
    static async Task Main(string[] args)
    {
      Console.WriteLine("Hello, Blink service!");

      IHost host = Host.CreateDefaultBuilder(args)
      .ConfigureServices((context, services) =>
      {
        var config = context.Configuration;

        var mapDbSection = config.GetSection("MapDatabase");
        services.Configure<MapDatabaseSettings>(mapDbSection);
        services.Configure<DaprSettings>(config.GetSection("DaprSettings"));

        services.AddSingleton<IMongoClient>(s =>
           new MongoClient(mapDbSection.Get<MapDatabaseSettings>().ConnectionString)
        );
        services.AddSingleton<ISubService, SubService>();
        services.AddSingleton<IPubService, PubService>();
        services.AddSingleton<IMapService, MapService>();
        services.AddSingleton<IStateService, StateService>();
        
        services.AddHostedService<HierarhyStateService>();
      })
      .Build();

      await host.RunAsync();
    }
  }
}
