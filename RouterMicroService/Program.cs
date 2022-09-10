using DbLayer;
using Domain;
using Domain.ServiceInterfaces;
using LeafletAlarmsRouter;
using Microsoft.Extensions.Configuration;
using RouterMicroService;

var builder = WebApplication.CreateBuilder(args);

var Configuration = builder.Configuration;
builder.Services.Configure<MapDatabaseSettings>(Configuration.GetSection("MapDatabase"));
builder.Services.Configure<RoutingSettings>(Configuration.GetSection("RoutingSettings"));

// Add services to the container.
builder.Services.AddSingleton<ILevelService, LevelService>();
builder.Services.AddSingleton<IRoutService, RoutService>();
builder.Services.AddSingleton<ITrackRouter, TrackRouter>();
builder.Services.AddHostedService<TimedHostedService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

app.Run();
