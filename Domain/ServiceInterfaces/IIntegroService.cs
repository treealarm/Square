
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain
{
  public interface IIntegroService
  {
    Task<Dictionary<string, IntegroDTO>> GetListByIdsAsync(List<string> ids);
    Task<Dictionary<string, IntegroDTO>> GetListByType(string i_name, string i_type);
  }
  internal interface IIntegroServiceInternal
  {
    Task UpdateListAsync(List<IntegroDTO> obj2UpdateIn);
    Task RemoveAsync(List<string> ids);    
  }

  public interface IIntegroTypesService
  {
    Task<Dictionary<string, IntegroTypeDTO>> GetTypesAsync(List<IntegroTypeKeyDTO> types);
  }
  internal interface IIntegroTypesInternal
  {
    Task UpdateTypesAsync(List<IntegroTypeDTO> types);
    Task RemoveTypesAsync(List<IntegroTypeKeyDTO> types);
  }
}
