
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain
{
  public interface IGroupUpdateService
  {
    public Task UpdateListAsync(List<GroupDTO> obj2UpdateIn);
    public Task RemoveAsync(List<string> ids);
    public Task RemoveByNameAsync(List<string> names);
  }
}
