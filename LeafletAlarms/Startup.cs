using DbLayer;
using Domain;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Swashbuckle.AspNetCore.Filters;
using System;
using System.Net;
using System.Net.WebSockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

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
      services.AddSingleton<MapService>();

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
          setUpAction.RoutePrefix = @"api";
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
        if (context.Request.Path == "/push")
        {
          if (context.WebSockets.IsWebSocketRequest)
          {
            using (WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync())
            {
              await Echo(context, webSocket);
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

        if (env.IsDevelopment())
        {
          spa.UseReactDevelopmentServer(npmScript: "start");
        }
      });

    }

    private async Task Echo(HttpContext context, WebSocket webSocket)
    {
      var buffer = new byte[1024 * 4];
      WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
      while (!result.CloseStatus.HasValue)
      {
        await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, CancellationToken.None);

        result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
      }
      await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
    }
    //End
  }
}
