using DbLayer.Services;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
        services.AddScoped(provider =>
        {
          //var http = provider.GetRequiredService<IHttpContextAccessor>();

          var contextProvider = provider.GetService<IRequestContextProvider>();
          if (contextProvider != null)
          {
            var realm = contextProvider.GetRealm();
          }          

          var cfg = provider.GetRequiredService<IConfiguration>();
          var settings = cfg.GetSection("MapDatabase").Get<MapDatabaseSettings>();

          var dbName = Environment.GetEnvironmentVariable("DB_REALM_NAME");

          if (string.IsNullOrEmpty(dbName))
          {
            throw new ArgumentException("Hey you!");
          }
          var username = Environment.GetEnvironmentVariable("POSTGRES_USER") ?? "keycloak";
          var password = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? "password";

          var builder = new NpgsqlConnectionStringBuilder
          {
            Host = settings.PgHost,
            Database = dbName,
            Username = username,
            Password = password,
            Port = settings.PgPort
          };

          var dataSourceBuilder = new NpgsqlDataSourceBuilder(builder.ConnectionString);
          dataSourceBuilder.EnableDynamicJson();
          dataSourceBuilder.UseNetTopologySuite();

          var dataSource = dataSourceBuilder.Build();

          return dataSource;
        });

        services.AddDbContext<PgDbContext>((sp, options) =>
        {
          var dataSource = sp.GetRequiredService<NpgsqlDataSource>();

          options.UseNpgsql(dataSource, npgsqlOptions =>
          {
            // Подключаем NetTopologySuite
            npgsqlOptions.UseNetTopologySuite();

            // Включаем retry на уровне EF Core
            npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: int.MaxValue,
                maxRetryDelay: TimeSpan.FromSeconds(1),
                errorCodesToAdd: null
            );


            // Сборка миграций
            npgsqlOptions.MigrationsAssembly(typeof(PgDbContext).Assembly.FullName);
          });

          // Логирование только ошибок, без всех SQL-запросов
          options.LogTo(Console.WriteLine,
                        new[] { DbLoggerCategory.Database.Command.Name },
                        LogLevel.Warning);

          //options.EnableSensitiveDataLogging(false);
          //options.LogTo(_ => { }, LogLevel.None);
        });
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
