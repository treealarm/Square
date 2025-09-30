using DbLayer.Services;
using Domain;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using System;

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

      {
        var your_username = Environment.GetEnvironmentVariable("POSTGRES_USER");
        var your_password = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD");
        var DbName = settings.DatabaseName;

        var builder = new NpgsqlConnectionStringBuilder
        {
          Host = settings.PgHost,
          Database = settings.DatabaseName,
          Username = your_username,
          Password = your_password,
          Port = settings.PgPort
        };

        string connectionString = builder.ConnectionString;

        var dataSourceBuilder = new NpgsqlDataSourceBuilder(builder.ConnectionString);
        dataSourceBuilder.EnableDynamicJson();
        dataSourceBuilder.UseNetTopologySuite();
        var dataSource = dataSourceBuilder.Build();
        services.AddSingleton(dataSource);
        services.AddDbContext<PgDbContext>();
      }      

      services.AddScoped<IEventsService, EventsService>();

      services.AddScoped<IMapService, MapService>();
      services.AddScoped<IGeoService, GeoService>();
      services.AddScoped<ITrackService, TrackService>();

      services.AddScoped<ILevelService, LevelService>();
      services.AddScoped<IStateService, StateService>();

      services.AddScoped<IDiagramTypeService, DiagramTypeService>();
      services.AddScoped<DiagramService>();
      services.AddScoped<IDiagramService>(provider => provider.GetRequiredService<DiagramService>());
      services.AddScoped<IDiagramServiceInternal>(provider => provider.GetRequiredService<DiagramService>());


      services.AddScoped<RightService>();
      services.AddScoped<IRightService>(provider => provider.GetRequiredService<RightService>());
      services.AddScoped<IRightServiceInternal>(provider => provider.GetRequiredService<RightService>());


      services.AddScoped<ValuesService>();
      services.AddScoped<IValuesService>(provider => provider.GetRequiredService<ValuesService>());
      services.AddScoped<IValuesServiceInternal>(provider => provider.GetRequiredService<ValuesService>());

      services.AddScoped<IntegroService>();
      services.AddScoped<IIntegroService>(provider => provider.GetRequiredService<IntegroService>());
      services.AddScoped<IIntegroServiceInternal>(provider => provider.GetRequiredService<IntegroService>());

      services.AddScoped<IIntegroTypesService>(provider => provider.GetRequiredService<IntegroService>());
      services.AddScoped<IIntegroTypesInternal>(provider => provider.GetRequiredService<IntegroService>());

      services.AddScoped<GroupsService>();
      services.AddScoped<IGroupsService>(provider => provider.GetRequiredService<GroupsService>());
      services.AddScoped<IIGroupsServiceInternal>(provider => provider.GetRequiredService<GroupsService>());

      services.AddScoped<ActionsService>();
      services.AddScoped<IActionsService>(provider => provider.GetRequiredService<ActionsService>());
      services.AddScoped<IActionsServiceInternal>(provider => provider.GetRequiredService<ActionsService>());
    }
  }
}
