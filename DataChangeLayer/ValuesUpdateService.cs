using Domain.ServiceInterfaces;
using Domain.Values;

namespace DataChangeLayer
{
  internal class ValuesUpdateService : IValuesUpdateService
  {
    private readonly IValuesServiceInternal _valuesService;

    private IPubService _pub;
    public ValuesUpdateService(
     IValuesServiceInternal valuesService,
     IPubService pub
    )
    {
      _valuesService = valuesService;
      _pub = pub;
    }
    public async Task RemoveValues(List<string> ids)
    {
      await _valuesService.RemoveAsync( ids );
    }

    public async Task UpdateValues(List<ValueDTO> obj2UpdateIn)
    {
      await _valuesService.UpdateListAsync( obj2UpdateIn );
    }
  }
}
