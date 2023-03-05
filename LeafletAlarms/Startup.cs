using DbLayer.Services;
using Domain;
using Domain.OptionsModels;
using Domain.ServiceInterfaces;
using Domain.StateWebSock;
using LeafletAlarms.Authentication;
using LeafletAlarms.Services;
using LeafletAlarmsRouter;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using PubSubLib;
using Swashbuckle.AspNetCore.Filters;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.WebSockets;
using System.Reflection;

namespace LeafletAlarms
{
    public class Startup
  {
    public Startup(IConfiguration configuration, IWebHostEnvironment env)
    {
      Configuration = configuration;
      _currentEnvironment = env;
    }

    public IConfiguration Configuration { get; }
    private readonly IWebHostEnvironment _currentEnvironment;
    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
      services.AddCors(options =>
      {
        options.AddDefaultPolicy(
            builder =>
            {
              builder.WithOrigins("https://localhost", "http://localhost")
                          .AllowAnyHeader()
                          .AllowAnyMethod(); //THIS LINE RIGHT HERE IS WHAT YOU NEED
            });
      });

      var keyCloackConf = Configuration.GetSection("KeyCloack");

      services.Configure<KeycloakSettings>(keyCloackConf);

      var kcConfiguration = keyCloackConf.Get<KeycloakSettings>();

      var realmName = kcConfiguration.RealmName;// keyCloackConf.GetValue<string>("RealmName");
      var PublicKeyJWT = kcConfiguration.PublicKeyJWT;// keyCloackConf.GetValue<string>("PublicKeyJWT");
      var BaseAddr = kcConfiguration.BaseAddr;// keyCloackConf.GetValue<string>("BaseAddr");

      //"http://localhost:8080/realms/myrealm"
      Uri? validIssuer;
      if (Uri.TryCreate(
        new Uri(BaseAddr, UriKind.Absolute),
        new Uri($"realms/{realmName}", UriKind.Relative),
        out validIssuer)
      )
      {

      }

      services.ConfigureJWT(_currentEnvironment.IsDevelopment(),
        PublicKeyJWT,//Realm settings/Keys/RS256(public)
        validIssuer.ToString()
      );

      services.Configure<MapDatabaseSettings>(Configuration.GetSection("MapDatabase"));
      services.Configure<RoutingSettings>(Configuration.GetSection("RoutingSettings"));
      services.Configure<DaprSettings>(Configuration.GetSection("DaprSettings"));


      services.AddHostedService<InitHostedService>();

      
      services.AddSingleton<IUtilService, UtilService>();

      services.AddSingleton<IPubSubService, PubSubService>();

      services.AddSingleton<IMapService, MapService>();
      services.AddSingleton<IGeoService, GeoService>();

      services.AddSingleton<ITrackRouter, TrackRouter>();
      services.AddSingleton<ITrackService, TrackService>();
      services.AddSingleton<IRoutService, RoutService>();
      services.AddSingleton<ILevelService, LevelService>();
      services.AddSingleton<IStateService, StateService>();
      services.AddSingleton<ILogicService, LogicService>();
      services.AddSingleton<ILogicProcessorService, FAKE_Service>();

      services.AddSingleton<ITrackConsumer, ConsumerService>();
      
      services.AddSingleton<IRightService, RightService>();
      services.AddSingleton<RightsCheckerService>();

      services.AddSingleton<IIdsQueue, StateQueueForUpdate>();
      services.AddHostedService<HierarhyStateService>();

      services.AddSingleton<ConsumerService>(); // We must explicitly register Foo
      services.AddSingleton<ITrackConsumer>(x => x.GetRequiredService<ConsumerService>()); // Forward requests to Foo
      services.AddSingleton<IStateConsumer>(x => x.GetRequiredService<ConsumerService>()); // Forward requests to Foo
      services.AddSingleton<IWebSockList>(x => x.GetRequiredService<ConsumerService>()); // Forward requests to Foo

      services.AddHttpContextAccessor();
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

        //First we define the security scheme
        setUpAction.AddSecurityDefinition("Bearer", //Name the security scheme
            new OpenApiSecurityScheme
            {
              Description = "JWT Authorization header using the Bearer scheme.",
              Type = SecuritySchemeType.Http, //We set the scheme type to http since we're using bearer authentication
              Scheme = JwtBearerDefaults.AuthenticationScheme //The name of the HTTP Authorization scheme to be used in the Authorization header. In this case "bearer".
            });

        setUpAction.AddSecurityRequirement(new OpenApiSecurityRequirement{
          {
              new OpenApiSecurityScheme{
                  Reference = new OpenApiReference{
                      Id = JwtBearerDefaults.AuthenticationScheme, //The name of the previously defined security scheme.
                      Type = ReferenceType.SecurityScheme
                  }
              },new List<string>()
          }
        });
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

      app.UseAuthorization();

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
              var handler = app.ApplicationServices.GetRequiredService<IWebSockList>();
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
