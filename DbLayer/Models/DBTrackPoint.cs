using DbLayer.Models;
using NetTopologySuite.Geometries;
using System;


namespace DbLayer
{
  internal record DBTrackPoint : BasePgEntity
  {
    public Guid? object_id { get; set; }

    public DateTime timestamp { get; set; }  // ts

    public Geometry figure { get; set; } // геометрия Point/LineString/Polygon
    public double? radius { get; set; }
    public string zoom_level { get; set; }

    public string extra_props { get; set; } // jsonb
  }
}
