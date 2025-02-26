using DbLayer.Services;
using Domain;
using Domain.OptionsModels;
using Domain.ServiceInterfaces;
using Google.Api;
using LeafletAlarmsRouter;
using Microsoft.OpenApi.Models;
using PubSubLib;
using RouterMicroService;
using Swashbuckle.AspNetCore.Filters;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

var Configuration = builder.Configuration;
builder.Services.Configure<MapDatabaseSettings>(Configuration.GetSection("MapDatabase"));
builder.Services.Configure<DaprSettings>(Configuration.GetSection("DaprSettings"));
builder.Services.Configure<RoutingSettings>(Configuration.GetSection("RoutingSettings"));

// Add services to the container.
builder.Services.AddDaprClient();
builder.Services.AddSingleton<IPubSubService, PubSubService>();
builder.Services.AddSingleton<ILevelService, LevelService>();
builder.Services.AddSingleton<IRoutService, RoutService>();
builder.Services.AddSingleton<ITrackRouter, TrackRouter>();
builder.Services.AddSingleton<ITrackService, TrackService>();
builder.Services.AddSingleton<IMapService, MapService>();
builder.Services.AddSingleton<IGeoService, GeoService>();

builder.Services.AddHostedService<TimedHostedService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(setUpAction =>
{
  setUpAction.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
  {
    Title = "RouterApi",
    Version = "1.0"
  });
  setUpAction.ExampleFilters();

  var xmlCommentsFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";

});
builder.Services.AddSwaggerExamplesFromAssemblies(Assembly.GetEntryAssembly());


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

try
{
  app.Run();
}
catch(Exception ex)
{
  Console.WriteLine(ex.ToString());
}

