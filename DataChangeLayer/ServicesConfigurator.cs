using Domain;
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
      services.AddScoped<IEventsUpdateService, EventsUpdateService>();
      services.AddSingleton<IRightUpdateService, RightUpdateService>();
      services.AddSingleton<IStatesUpdateService, StatesUpdateService>();
      
      services.AddSingleton<IValuesUpdateService, ValuesUpdateService>();
      services.AddSingleton<IIntegroUpdateService, IntegroUpdateService>();
      services.AddSingleton<IGroupUpdateService, GroupUpdateService>();
    }
   }
}
