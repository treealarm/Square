using Domain.ServiceInterfaces;

namespace DataChangeLayer
{
  public class DataChangeService: IDataChangeService
  {
    private readonly IMapService _mapService;
    public DataChangeService(IMapService mapService)
    {
      _mapService = mapService;
    }
  }
}
