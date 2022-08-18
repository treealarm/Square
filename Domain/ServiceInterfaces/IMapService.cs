using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.ServiceInterfaces
{
  public interface IMapService
  {
    Task<List<BaseMarkerDTO>> GetAsync(List<string> ids);

    Task<BaseMarkerDTO> GetAsync(string id);
    Task<List<BaseMarkerDTO>> GetByChildIdAsync(string object_id);
    Task<List<BaseMarkerDTO>> GetByParentIdAsync(string parent_id);
    Task<List<BaseMarkerDTO>> GetByNameAsync(string name);
    Task<List<BaseMarkerDTO>> GetTopChildren(List<string> parentIds);
    Task<List<BaseMarkerDTO>> GetAllChildren(string parent_id);
    Task<ObjPropsDTO> GetPropAsync(string id);
    Task CreateOrUpdateHierarchyObject(BaseMarkerDTO marker);
    Task UpdatePropAsync(ObjPropsDTO updatedObj);
    Task UpdateHierarchyAsync(BaseMarkerDTO updatedObj);
    Task<long> RemoveAsync(List<string> ids);
  }
}
