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
      var envVars = new Dictionary<string, string>();

      var environmentVariables = Environment.GetEnvironmentVariables();

      foreach (var variable in environmentVariables.Keys)
      {
        var key = variable?.ToString().ToLower();

        if (
          key == "botid" ||
          key == "chatid")
        {
          envVars.Add(key, environmentVariables[variable].ToString());
        }
      }

      var config = context.Configuration;

      services.Configure<MapDatabaseSettings>(config.GetSection("MapDatabase"));
      services.AddSingleton<IPubSubService, PubSubService>();
    })
    .Build();

await host.RunAsync();
