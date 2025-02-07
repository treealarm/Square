
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain
{
  public interface IRightUpdateService
  {
    public Task<long> Delete(string id);
    public Task<List<ObjectRightsDTO>> Update(List<ObjectRightsDTO> newObjs);
  }
}
