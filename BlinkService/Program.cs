using Dapr.Messaging.PublishSubscribe.Extensions;
using Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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

        services.AddDaprPubSubClient();
        services.AddSingleton<ISubService, SubService>();
        services.AddSingleton<IPubService, PubService>();
        DbLayer.ServicesConfigurator.ConfigureServices(services, config);
        DataChangeLayer.ServicesConfigurator.ConfigureServices(services);
        services.AddHostedService<HierarhyStateService>();
      })
      .Build();

      await host.RunAsync();
    }
  }
}
