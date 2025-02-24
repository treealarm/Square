using DbLayer.Services;
using Domain;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace DbLayer
{
  public static class ServicesConfigurator
  {
    public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
      var mapDbSection = configuration.GetSection("MapDatabase");

      // Корректная регистрация IOptions<MapDatabaseSettings>
      services.Configure<MapDatabaseSettings>(options => mapDbSection.Bind(options));

      // Загружаем объект вручную, если нужно
      var settings = mapDbSection.Get<MapDatabaseSettings>();

      services.AddSingleton<IMongoClient>(s =>
          new MongoClient(settings.ConnectionString) // Используем settings напрямую
      );

      services.AddDbContext<PgDbContext>();

      services.AddScoped<IEventsService, PgEventsService>();

      services.AddSingleton<IUtilService, UtilService>();

      services.AddSingleton<IMapService, MapService>();
      services.AddSingleton<IGeoService, GeoService>();
      services.AddSingleton<ITrackService, TrackService>();
      services.AddSingleton<IRoutService, RoutService>();
      services.AddSingleton<ILevelService, LevelService>();
      services.AddSingleton<IStateService, StateService>();

      services.AddSingleton<IDiagramTypeService, DiagramTypeService>();
      services.AddSingleton<DiagramService>();
      services.AddSingleton<IDiagramService>(provider => provider.GetRequiredService<DiagramService>());
      services.AddSingleton<IDiagramServiceInternal>(provider => provider.GetRequiredService<DiagramService>());


      services.AddSingleton<IRightService, RightService>();
      //services.AddSingleton<IEventsService, EventsService>();


      services.AddSingleton<ValuesService>();
      services.AddSingleton<IValuesService>(provider => provider.GetRequiredService<ValuesService>());
      services.AddSingleton<IValuesServiceInternal>(provider => provider.GetRequiredService<ValuesService>());

      services.AddSingleton<IntegroService>();
      services.AddSingleton<IIntegroService>(provider => provider.GetRequiredService<IntegroService>());
      services.AddSingleton<IIntegroServiceInternal>(provider => provider.GetRequiredService<IntegroService>());

      services.AddSingleton<GroupsService>();
      services.AddSingleton<IGroupsService>(provider => provider.GetRequiredService<GroupsService>());
      services.AddSingleton<IIGroupsServiceInternal>(provider => provider.GetRequiredService<GroupsService>());
    }
  }
}
