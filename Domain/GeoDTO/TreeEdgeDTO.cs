using Domain.GeoDBDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.GeoDTO
{
  public class TreeEdgeDTO
  {
    public uint EdgeId { get; set; }

    public uint PreviousEdgeId { get; set; }

    public float Weight1 { get; set; }

    public float Weight2 { get; set; }

    public Geo2DCoordDTO? Shape { get; set; }

    public HashSet<TreeEdgeDTO>? Children { get; set; }

    public override int GetHashCode()
    {
      return (int)EdgeId;
    }

    public override bool Equals(object? obj)
    {
      if ( obj is TreeEdgeDTO treeEdge)
      {
        return treeEdge?.EdgeId == EdgeId;
      }
      return base.Equals(obj);
    }
  }
}
