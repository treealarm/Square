using Domain.ServiceInterfaces;
using Microsoft.Extensions.DependencyInjection;

namespace DataChangeLayer
{
  public class ServicesConfigurator
  {
    public static void ConfigureServices(IServiceCollection services)
    {
      services.AddSingleton<ITracksUpdateService, TracksUpdateService>();      
      services.AddSingleton<IMapUpdateService, MapUpdateService>();
      services.AddSingleton<IDiagramUpdateService, DiagramUpdateService>();
      services.AddSingleton<IDiagramTypeUpdateService, DiagramTypeUpdateService>();
      services.AddSingleton<IEventsUpdateService, EventsUpdateService>();
      services.AddSingleton<IRightUpdateService, RightUpdateService>();
      services.AddSingleton<IStatesUpdateService, StatesUpdateService>();
      
      services.AddSingleton<IValuesUpdateService, ValuesUpdateService>();
    }
   }
}
