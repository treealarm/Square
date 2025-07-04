﻿using Domain;
using Microsoft.Extensions.DependencyInjection;

namespace DataChangeLayer
{
  public class ServicesConfigurator
  {
    public static void ConfigureServices(IServiceCollection services)
    {
      services.AddSingleton<ITracksUpdateService, TracksUpdateService>();      
      services.AddScoped<IMapUpdateService, MapUpdateService>();
      services.AddSingleton<IDiagramUpdateService, DiagramUpdateService>();
      services.AddSingleton<IDiagramTypeUpdateService, DiagramTypeUpdateService>();
      services.AddScoped<IEventsUpdateService, EventsUpdateService>();
      services.AddSingleton<IRightUpdateService, RightUpdateService>();
      services.AddScoped<IStatesUpdateService, StatesUpdateService>();
      
      services.AddScoped<IValuesUpdateService, ValuesUpdateService>();
      services.AddSingleton<IGroupUpdateService, GroupUpdateService>();

      services.AddScoped<IntegroUpdateService>();
      services.AddScoped<IIntegroUpdateService>(provider => provider.GetRequiredService<IntegroUpdateService>());
      services.AddScoped<IIntegroTypeUpdateService>(provider => provider.GetRequiredService<IntegroUpdateService>());

      services.AddScoped<IActionsUpdateService, ActionsUpdateService>();
    }
   }
}
