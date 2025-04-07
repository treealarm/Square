using Domain;

namespace DataChangeLayer
{
  internal class IntegroUpdateService: IIntegroUpdateService, IIntegroTypeUpdateService
  {
    private readonly IIntegroServiceInternal _integroService;
    private readonly IIntegroTypesInternal _integroTypesService;

    private IPubService _pub;
    public IntegroUpdateService(
     IIntegroServiceInternal integroService,
     IIntegroTypesInternal integroTypeService,
     IPubService pub
    )
    {
      _integroService = integroService;
      _integroTypesService = integroTypeService;
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

    public async Task UpdateTypesAsync(List<IntegroTypeDTO> types)
    {
      await _integroTypesService.UpdateTypesAsync(types);
    }

    public async Task RemoveTypesAsync(List<IntegroTypeKeyDTO> types)
    {
      await _integroTypesService.RemoveTypesAsync(types);
    }
  }
}
