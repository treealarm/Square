
using Domain.Rights;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.ServiceInterfaces
{
  public interface IRightUpdateService
  {
    public Task<long> Delete(string id);
    public Task<List<ObjectRightsDTO>> Update(List<ObjectRightsDTO> newObjs);
  }
}
