
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain
{
  public interface IRightService
  {
    public Task<Dictionary<string, List<ObjectRightValueDTO>>> GetListByIdsAsync(List<string> ids);
  }
  public interface IRightUpdateService
  {
    public Task<long> Delete(string id);
    public Task<List<ObjectRightValueDTO>> Update(List<ObjectRightValueDTO> newObjs);
  }
  internal interface IRightServiceInternal: IRightService, IRightUpdateService
  {
  }
}
