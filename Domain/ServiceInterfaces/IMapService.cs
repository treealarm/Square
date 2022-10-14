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
    Task<List<BaseMarkerDTO>> GetByParentIdAsync(
      string parent_id,
      string start_id,
      string end_id,
      int count
    );
    Task<List<BaseMarkerDTO>> GetByNameAsync(string name);
    Task<List<BaseMarkerDTO>> GetTopChildren(List<string> parentIds);
    Task<List<BaseMarkerDTO>> GetAllChildren(string parent_id);
    Task<ObjPropsDTO> GetPropAsync(string id);
    Task<List<ObjPropsDTO>> GetPropsAsync(List<string> ids);
    Task UpdatePropAsync(ObjPropsDTO updatedObj);
    Task UpdateHierarchyAsync(IEnumerable<BaseMarkerDTO> updatedObj);
    Task<long> RemoveAsync(List<string> ids);
    Task<List<ObjPropsDTO>> GetPropByValuesAsync(
      ObjPropsSearchDTO propFilter,
      string start_id,
      bool forward,
      int count
    );
    Task UpdatePropNotDeleteAsync(IEnumerable<FigureBaseDTO> updatedObj);
  }
}
