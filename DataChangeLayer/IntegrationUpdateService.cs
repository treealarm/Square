using Domain.Integration;
using Domain.ServiceInterfaces;

namespace DataChangeLayer
{
  internal class IntegrationLeafsUpdateService: IIntegrationLeafsUpdateService
  {
    private readonly IIntegrationLeafsServiceInternal _integrationService;

    private IPubService _pub;
    public IntegrationLeafsUpdateService(
      IIntegrationLeafsServiceInternal integrationService,
      IPubService pub
    )
    {
      _integrationService = integrationService;
      _pub = pub;
    }
    async Task IIntegrationLeafsUpdateService.UpdateListAsync(List<IntegrationLeafsDTO> obj2UpdateIn)
    {
      await _integrationService.UpdateListAsync(obj2UpdateIn);
    }
    async Task IIntegrationLeafsUpdateService.RemoveAsync(List<string> ids)
    {
      await _integrationService.RemoveAsync(ids);
    }
  }
}
