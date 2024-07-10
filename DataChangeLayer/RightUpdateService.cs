
using Domain.Rights;
using Domain.ServiceInterfaces;

namespace DataChangeLayer
{
  internal class RightUpdateService : IRightUpdateService
  {
    private readonly IRightService _rightsService;
    public RightUpdateService(
     IRightService rightsService
     )
    {
      _rightsService = rightsService;
    }
    public async Task<long> Delete(string id)
    {
       return await _rightsService.DeleteAsync(id);
    }

    public async Task<List<ObjectRightsDTO>> Update(List<ObjectRightsDTO> newObjs)
    {
      await _rightsService.UpdateListAsync(newObjs);
      return newObjs;
    }
  }
}
