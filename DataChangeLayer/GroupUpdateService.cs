using Domain;

namespace DataChangeLayer
{
  internal class GroupUpdateService: IGroupUpdateService
  {
    private readonly IIGroupsServiceInternal _internalService;

    private IPubService _pub;
    public GroupUpdateService(
     IIGroupsServiceInternal internalService,
     IPubService pub
    )
    {
      _internalService = internalService;
      _pub = pub;
    }

    public async Task RemoveAsync(List<string> ids)
    {
      await _internalService.RemoveAsync( ids );
    }

    public async Task RemoveByNameAsync(List<string> names)
    {
     await _internalService.RemoveByNameAsync( names );
    }

    public async Task UpdateListAsync(List<GroupDTO> obj2UpdateIn)
    {
      await _internalService.UpdateListAsync( obj2UpdateIn );
    }
  }
}
