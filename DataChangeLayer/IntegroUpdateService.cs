using Domain;

namespace DataChangeLayer
{
  internal class IntegroUpdateService: IIntegroUpdateService, IIntegroTypeUpdateService
  {
    private readonly IIntegroServiceInternal _integroServiceInternal;
    private readonly IIntegroTypesInternal _integroTypesService;
    private readonly IIntegroService _integroService;

    private IPubService _pub;
    public IntegroUpdateService(
     IIntegroServiceInternal integroServiceInternal,
     IIntegroTypesInternal integroTypeService,
     IIntegroService integroService,
     IPubService pub
    )
    {
      _integroServiceInternal = integroServiceInternal;
      _integroTypesService = integroTypeService;
      _integroService = integroService;
      _pub = pub;
    }

    public async Task RemoveIntegros(List<string> ids)
    {
      await OnUpdatedNormalObjects(ids, Topics.OnDeleteIntegros);
      await _integroServiceInternal.RemoveAsync(ids);
    }

    private async Task OnUpdateIntegros(List<IntegroDTO> obj2UpdateIn, string topic)
    {
      var i_names = obj2UpdateIn.Select(o => o.i_name).Distinct().ToList();

      foreach (var i in i_names)
      {
        var objects = obj2UpdateIn
          .Where(o => o.i_name == i)
          .Select(o => o.id)
          .ToList();
        await _pub.Publish($"{topic}_{i}", objects);
      }
    }
    public async Task UpdateIntegros(List<IntegroDTO> obj2UpdateIn)
    {      
      await _integroServiceInternal.UpdateListAsync(obj2UpdateIn);
      await OnUpdateIntegros(obj2UpdateIn, Topics.OnUpdateIntegros);
    }

    public async Task UpdateTypesAsync(List<IntegroTypeDTO> types)
    {
      await _integroTypesService.UpdateTypesAsync(types);
    }

    public async Task RemoveTypesAsync(List<IntegroTypeKeyDTO> types)
    {      
      await _integroTypesService.RemoveTypesAsync(types);
    }

    public async Task OnUpdatedNormalObjects(List<string> ids, string topic)
    {
      var dic = await _integroService.GetListByIdsAsync(ids);
      var obj2UpdateIn = dic.Values.ToList();
      await OnUpdateIntegros(obj2UpdateIn, topic);
    }
  }
}
