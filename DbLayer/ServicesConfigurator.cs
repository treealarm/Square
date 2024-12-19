using DbLayer.Services;
using Domain.ServiceInterfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbLayer
{
  public static class ServicesConfigurator
  {
    public static void ConfigureServices(IServiceCollection services)
    {
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
      services.AddSingleton<IEventsService, EventsService>();


      services.AddSingleton<ValuesService>();
      services.AddSingleton<IValuesService>(provider => provider.GetRequiredService<ValuesService>());
      services.AddSingleton<IValuesServiceInternal>(provider => provider.GetRequiredService<ValuesService>());

      services.AddSingleton<IntegroService>();
      services.AddSingleton<IIntegroService>(provider => provider.GetRequiredService<IntegroService>());
      services.AddSingleton<IIntegroServiceInternal>(provider => provider.GetRequiredService<IntegroService>());

    }
  }
}
