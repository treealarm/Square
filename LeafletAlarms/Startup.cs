using DbLayer.Services;
using Domain;
using Domain.ServiceInterfaces;
using Domain.StateWebSock;
using LeafletAlarms.Services;
using LeafletAlarmsRouter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Swashbuckle.AspNetCore.Filters;
using System;
using System.Net;
using System.Net.WebSockets;
using System.Reflection;

namespace LeafletAlarms
{
  public class Startup
  {
    public Startup(IConfiguration configuration)
    {
      Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
      services.Configure<MapDatabaseSettings>(Configuration.GetSection("MapDatabase"));
      var rt = Configuration.GetSection("RoutingSettings");
      var routingSettings = rt.Get(typeof(RoutingSettings)) as RoutingSettings;

      if (!InDocker)
      {
        routingSettings.RoutingFilePath = routingSettings.RoutingFilePathWin;
      }
      
      services.Configure<RoutingSettings>(Configuration.GetSection("RoutingSettings"));

      services.AddHostedService<InitHostedService>();

      services.AddSingleton<IMapService, MapService>();
      services.AddSingleton<IGeoService, GeoService>();

      services.AddSingleton<ITrackRouter, TrackRouter>();
      services.AddSingleton<ITrackService, TrackService>();
      services.AddSingleton<IRoutService, RoutService>();
      services.AddSingleton<ILevelService, LevelService>();
      services.AddSingleton<IStateService, StateService>();
      services.AddSingleton<ITrackConsumer, StateWebSocketHandler>();
      

      services.AddSingleton<IIdsQueue, StateQueueForUpdate>();
      services.AddHostedService<HierarhyStateService>();

      services.AddSingleton<StateWebSocketHandler>(); // We must explicitly register Foo
      services.AddSingleton<ITrackConsumer>(x => x.GetRequiredService<StateWebSocketHandler>()); // Forward requests to Foo
      services.AddSingleton<IStateConsumer>(x => x.GetRequiredService<StateWebSocketHandler>()); // Forward requests to Foo
      
      
      services.AddControllersWithViews();

      // In production, the React files will be served from this directory
      services.AddSpaStaticFiles(configuration =>
      {
        configuration.RootPath = "ClientApp/build";
      });

      services.AddSwaggerGen(setUpAction =>
      {
        setUpAction.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
        {
          Title = "MapApi",
          Version = "1.0"
        });
        setUpAction.ExampleFilters();

        var xmlCommentsFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        //setUpAction.IncludeXmlComments(xmlCommentsFile);
        //setUpAction.AddFluentValidationRules();
      });
      services.AddSwaggerExamplesFromAssemblies(Assembly.GetEntryAssembly());
    }

    private bool InDocker 
    { 
      get 
      { 
        return Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true"; 
      } 
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
        app.UseSwagger();

        app.UseSwaggerUI(setUpAction =>
        {
          setUpAction.SwaggerEndpoint(@"/swagger/v1/swagger.json", "Map API");
          setUpAction.RoutePrefix = @"swagger";
        });
      }
      else
      {
        app.UseExceptionHandler("/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
      }

      app.UseHttpsRedirection();
      app.UseStaticFiles();
      app.UseSpaStaticFiles();

      app.UseRouting();

      app.UseEndpoints(endpoints =>
      {
        endpoints.MapControllerRoute(
                  name: "default",
                  pattern: "{controller}/{action=Index}/{id?}");
      });

      var webSocketOptions = new WebSocketOptions
      {
        KeepAliveInterval = TimeSpan.FromMinutes(2)
      };

      app.UseWebSockets(webSocketOptions);

      app.Use(async (context, next) =>
      {
        if (context.Request.Path == "/state")
        {
          if (context.WebSockets.IsWebSocketRequest)
          {
            using (WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync())
            {
              var handler = app.ApplicationServices.GetRequiredService<ITrackConsumer>();
              await handler.PushAsync(context, webSocket);
            }
          }
          else
          {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
          }
        }
        else
        {
          await next();
        }

      });

      app.UseSpa(spa =>
      {
        spa.Options.SourcePath = "ClientApp";

        if (env.IsDevelopment() && !InDocker)
        {
          spa.UseReactDevelopmentServer(npmScript: "start");
        }
      });

    }
    //End
  }
}
