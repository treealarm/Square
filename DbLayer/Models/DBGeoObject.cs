using DbLayer.Models;
using NetTopologySuite.Geometries;

namespace DbLayer
{
  internal record DBGeoObject : BasePgEntity
  {
    public Geometry figure { get; set; } // NetTopologySuite
    public double? radius { get; set; }
    public string zoom_level { get; set; }
  }
}
