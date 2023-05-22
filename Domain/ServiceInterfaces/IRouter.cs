using Domain.GeoDBDTO;
using Domain.GeoDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.ServiceInterfaces
{
  public interface ITrackRouter
  {
    Task<List<Geo2DCoordDTO>> GetRoute(
      string inst,
      string profile,
      List<Geo2DCoordDTO> coords);

    public List<TreeEdgeDTO> CalculateTree(
      string inst,
      string strProfile,
      Geo2DCoordDTO coord,
      int max
    );
    public bool IsMapExist(string inst);
    public void RemoveEdges(string inst, string profileName, HashSet<uint> toRemove);
    public void SetLowWeight(string inst, string profileName, List<Geo2DCoordDTO> coords, double weight);
  }
}
