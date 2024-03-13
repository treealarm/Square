// See https://aka.ms/new-console-template for more information
using Domain.OptionsModels;
using Domain.ServiceInterfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PubSubLib;

Console.WriteLine("Hello, DB Watcher!");

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
      var config = context.Configuration;
      var mapSection = config.GetSection("MapDatabase");
      var secVal = mapSection.GetSection("DatabaseName").Value;
      services.Configure<MapDatabaseSettings>(mapSection);      
    })
    .Build();

await host.RunAsync();
