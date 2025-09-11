
using Domain;


namespace DataChangeLayer
{
  internal class RightUpdateService : IRightUpdateService
  {
    private readonly IRightServiceInternal _rightsService;
    public RightUpdateService(
     IRightServiceInternal rightsService
     )
    {
      _rightsService = rightsService;
    }
    public async Task<long> Delete(string id)
    {
       return await _rightsService.Delete(id);
    }

    public async Task<List<ObjectRightValueDTO>> Update(List<ObjectRightValueDTO> newObjs)
    {
      await _rightsService.Update(newObjs);
      return newObjs;
    }
  }
}
