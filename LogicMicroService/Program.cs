using DbLayer.Services;
using Domain.OptionsModels;
using Domain.ServiceInterfaces;
using LogicMicroService;
using PubSubLib;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
      var config = context.Configuration;

      services.Configure<MapDatabaseSettings>(config.GetSection("MapDatabase"));
      services.Configure<DaprSettings>(config.GetSection("DaprSettings"));      

      services.AddSingleton<ISubService, SubService>();
      services.AddSingleton<IPubService, PubService>();
      services.AddSingleton<ILevelService, LevelService>();
      services.AddSingleton<ILogicService, LogicService>();
      services.AddSingleton<IMapService, MapService>();
      services.AddSingleton<IGeoService, GeoService>();
      services.AddSingleton<ITrackService, TrackService>();
      services.AddHostedService<LogicProcessorHost>();

    })
    .Build();

await host.RunAsync();
