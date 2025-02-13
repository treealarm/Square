using Domain;

namespace DataChangeLayer
{
  internal class IntegroUpdateService: IIntegroUpdateService
  {
    private readonly IIntegroServiceInternal _integroService;

    private IPubService _pub;
    public IntegroUpdateService(
     IIntegroServiceInternal integroService,
     IPubService pub
    )
    {
      _integroService = integroService;
      _pub = pub;
    }

    public async Task RemoveIntegros(List<string> ids)
    {
      await _integroService.RemoveAsync(ids);
    }

    public async Task UpdateIntegros(List<IntegroDTO> obj2UpdateIn)
    {
      await _integroService.UpdateListAsync(obj2UpdateIn);
    }
  }
}
