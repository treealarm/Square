using DataChangeLayer;
using DbLayer.Services;
using Domain;
using Domain.OptionsModels;
using Domain.ServiceInterfaces;
using Domain.StateWebSock;
using LeafletAlarms.Authentication;
using LeafletAlarms.Grpc.Implementation;
using LeafletAlarms.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using PubSubLib;
using Swashbuckle.AspNetCore.Filters;
using System.Net;
using System.Net.WebSockets;
using System.Reflection;
using System.Text.Json;
using ValhallaLib;

namespace LeafletAlarms
{
  public class Startup
  {
    public IConfiguration Configuration { get; }

    public Startup(
      IConfiguration configuration
    )
    {
      Configuration = configuration;
    }


    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
      var keyCloackConf = Configuration.GetSection("KeyCloack");

      services.Configure<KeycloakSettings>(keyCloackConf);

      var kcConfiguration = keyCloackConf.Get<KeycloakSettings>();
      services.AddSingleton<KeyCloakConnectorService>();

      services.ConfigureJWT();

      var mapDbSection = Configuration.GetSection("MapDatabase");
      services.Configure<MapDatabaseSettings>(mapDbSection);
      services.Configure<RoutingSettings>(Configuration.GetSection("RoutingSettings"));
      services.Configure<DaprSettings>(Configuration.GetSection("DaprSettings"));


      services.AddSingleton<IMongoClient>(s =>
          new MongoClient(mapDbSection.Get<MapDatabaseSettings>().ConnectionString)
      );


      services.AddHostedService<InitHostedService>();

      
      services.AddSingleton<IUtilService, UtilService>();

      services.AddSingleton<ISubService, SubService>();
      services.AddSingleton<IPubService, PubService>();

      services.AddSingleton<IMapService, MapService>();
      services.AddSingleton<IGeoService, GeoService>();
      services.AddSingleton<ITrackService, TrackService>();
      services.AddSingleton<IRoutService, RoutService>();
      services.AddSingleton<ILevelService, LevelService>();
      services.AddSingleton<IStateService, StateService>();

      services.AddSingleton<IDiagramTypeService, DiagramTypeService>();
      services.AddSingleton<IDiagramService, DiagramService>();
    
      services.AddSingleton<IRightService, RightService>();
      services.AddSingleton<RightsCheckerService>();

      services.AddSingleton<IEventsService, EventsService>();

      services.AddHostedService<HierarhyStateService>();

      services.AddSingleton<WebSockListService>(); // We must explicitly register Foo
      services.AddSingleton<IWebSockList>(x => x.GetRequiredService<WebSockListService>()); // Forward requests to Foo
      services.AddSingleton<ITracksUpdateService, TracksUpdateService>();
      services.AddSingleton<StatesUpdateService>();

      services.AddSingleton<GRPCServiceProxy>();
      services.AddSingleton<FileSystemService>();

      services.AddSingleton<ValhallaRouter>();
      services.AddSingleton<IMapUpdateService, MapUpdateService>();
      services.AddSingleton<IDiagramUpdateService, DiagramUpdateService>();
      services.AddSingleton<IDiagramTypeUpdateService, DiagramTypeUpdateService>();
      services.AddSingleton<IEventsUpdateService, EventsUpdateService>();      

      services.AddHttpContextAccessor();
      services.AddControllersWithViews();
      services.AddEndpointsApiExplorer();
      services.AddSwaggerGen(setUpAction =>
      {
        setUpAction.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
        {
          Title = "MapApi",
          Version = "1.0"
        });
        setUpAction.ExampleFilters();

        var xmlCommentsFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";

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

    public static bool InDocker 
    { 
      get 
      { 
        return Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true"; 
      } 
    }

    public DirectoryInfo GetRootFolder()
    {
      var options = Configuration.GetSection("RoutingSettings").Get<RoutingSettings>();

      try
      {
        Directory.CreateDirectory(options.RootFolder);
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.ToString());
      }

      var dataDirectory = new DirectoryInfo(options.RootFolder);

      Console.WriteLine($"dataDirectory:{options.RootFolder}");

      if (!dataDirectory.Exists)
      {
        return null;
      }
      return dataDirectory;
    }

    public string GetAlternativePublicFolder()
    {
      var dataDirectory = GetRootFolder();

      if (dataDirectory == null)
      {
        return null;
      }

      var path = dataDirectory.FullName;
      path = Path.Combine(path, "wwwpublic");

      try
      {
        Directory.CreateDirectory(path);
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.ToString());
      }

      return path;
    }

    private async Task  ImportDataOnStart(WebApplication app)
    {
      var rootFolder = GetRootFolder();

      if (rootFolder != null)
      { 
        var path = rootFolder.FullName;

        path = Path.Combine(path, "import_data");

        try
        {
          Directory.CreateDirectory(path);
        }
        catch (Exception ex)
        {
          Console.WriteLine(ex.ToString());
        }

        var file2Import = Path.Combine(path, "states.json");

        if (File.Exists(file2Import))
        {
          try
          {
            FiguresDTO json = null;
            var s = File.ReadAllText(file2Import);
            json = JsonSerializer.Deserialize<FiguresDTO>(s);
            var trackUpdate = app.Services.GetRequiredService<TracksUpdateService>();
            await trackUpdate.UpdateFigures(json);
            FileInfo fileInfo = new FileInfo(file2Import);
            fileInfo.MoveTo(fileInfo.Directory.FullName + "\\" + "states_done.json");
          }
          catch (Exception ex)
          {
            Console.WriteLine(ex.Message);
            return;
          }
        }
      }
    }
    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(WebApplication app, IWebHostEnvironment env)
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

      app.UseCors(builder => builder
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader());

      app.UseStaticFiles(); // For default.Important to load index.html from wwwroot
      var staticFilesPath = Path.Combine(GetRootFolder().FullName, "static_files");

      try
      {
        Directory.CreateDirectory(staticFilesPath);
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.ToString());
      }

      app.UseFileServer(new FileServerOptions
      {
        FileProvider = new CompositeFileProvider(
        new PhysicalFileProvider(staticFilesPath)
        ),
        RequestPath = "/static_files",
        EnableDirectoryBrowsing = true
      });

      var keycloak_json_folder = GetAlternativePublicFolder();
      var wwwrootFolder = Path.Combine(env.ContentRootPath, "wwwroot");
      Console.WriteLine($"keycloak_json_folder: {keycloak_json_folder} -> {wwwrootFolder}");

      if (!string.IsNullOrEmpty(keycloak_json_folder)
        && !string.IsNullOrEmpty(wwwrootFolder)
        && Directory.Exists(wwwrootFolder)
      )
      {
        Console.WriteLine($"Copy files: {keycloak_json_folder} -> {wwwrootFolder}");

        var files = Directory.GetFiles(keycloak_json_folder).ToList();

        foreach( var f in files )
        {
          try
          {
            Console.WriteLine($"Copy file: {Path.GetFileName(f)}");
            File.Copy(f, Path.Combine(wwwrootFolder, Path.GetFileName(f)), true);
          }
          catch( Exception ex )
          {
            Console.WriteLine( ex.ToString() );
          }          
        }
      }

      Task.Run(async Task () =>
      {
        await ImportDataOnStart(app);
      });
      

      app.UseRouting();
      app.UseAuthorization();

      app.MapControllerRoute(
        name: "default",
        pattern: "{controller}/{action=Index}/{id?}");

      var webSocketOptions = new WebSocketOptions
      {
        KeepAliveInterval = TimeSpan.FromMinutes(2)
      };

      app.UseWebSockets(webSocketOptions);

      app.Use(async (context, next) =>
      {
        try
        {
          if (context.Request.Path == "/state")
          {
            if (context.WebSockets.IsWebSocketRequest)
            {
              using (WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync())
              {
                var handler = app.Services.GetRequiredService<IWebSockList>();
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
        }
        catch ( Exception ex )
        {
          Console.WriteLine("App USE Error:" + ex.ToString() );
        }
        
      });
      app.MapControllers();
      app.MapFallbackToFile("/index.html");
    }
    //End
  }
}
