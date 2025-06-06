﻿using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain
{
  public interface IMapService
  {
    Task<Dictionary<string, BaseMarkerDTO>> GetAsync(List<string> ids);
    Task<Dictionary<string, BaseMarkerDTO>> GetOwnersAsync(List<string> ids);
    Task<Dictionary<string, BaseMarkerDTO>> GetOwnersAndViewsAsync(List<string> ids);
    Task<BaseMarkerDTO> GetAsync(string? id);
    Task<List<BaseMarkerDTO>> GetByChildIdAsync(string object_id);
    Task<Dictionary<string, BaseMarkerDTO>> GetByParentIdAsync(
      string parent_id,
      string start_id,
      string end_id,
      int count
    );

    Task<Dictionary<string, BaseMarkerDTO>> GetByParentIdsAsync(
      List<string> parent_ids,
      string start_id,
      string end_id,
      int count
    );

    Task<Dictionary<string, BaseMarkerDTO>> GetByNameAsync(string name);
    Task<Dictionary<string, BaseMarkerDTO>> GetTopChildren(List<string> parentIds);
    Task<List<BaseMarkerDTO>> GetAllChildren(string parent_id);
    Task<ObjPropsDTO> GetPropAsync(string id);
    Task<Dictionary<string, ObjPropsDTO>> GetPropsAsync(List<string> ids);
    Task UpdatePropAsync(IEnumerable<ObjPropsDTO> updatedObj);
    Task UpdateHierarchyAsync(IEnumerable<BaseMarkerDTO> updatedObj);
    Task<long> RemoveAsync(List<string> ids);
    Task<List<ObjPropsDTO>> GetPropByValuesAsync(
      ObjPropsSearchDTO propFilter,
      string start_id,
      int forward,
      int count
    );
    Task UpdatePropNotDeleteAsync(IEnumerable<IObjectProps> updatedObj);
    Task<FiguresDTO> GetFigures(Dictionary<string, GeoObjectDTO> geo);
  }
}
