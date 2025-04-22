
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain
{
  public interface IIntegroUpdateService
  {
    public Task UpdateIntegros(List<IntegroDTO> obj2UpdateIn);
    public Task RemoveIntegros(List<string> ids);
    public Task OnUpdatedNormalObjects(List<string> ids, string topic);
  }
  public interface IIntegroTypeUpdateService
  {
    Task UpdateTypesAsync(List<IntegroTypeDTO> types);
    Task RemoveTypesAsync(List<IntegroTypeKeyDTO> types);
  }

}
