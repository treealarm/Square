
using Domain;

namespace DataChangeLayer
{
  internal class RoutUpdateService : IRoutUpdateService
  {
    private IRoutService _routService;
    public RoutUpdateService(IRoutService routService)
    {
      _routService = routService;
    }

    public async Task<long> InsertRoutes(List<RoutLineDTO> newObjs)
    {
        return await _routService.InsertManyAsync(newObjs);
    }
  }
}
