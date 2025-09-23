using NetTopologySuite.Geometries;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace DbLayer
{
  internal class DBTrackPoint
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid id { get; set; } // unique id
    public Guid? object_id { get; set; }

    public DateTime timestamp { get; set; }  // ts

    public Geometry figure { get; set; } // геометрия Point/LineString/Polygon
    public double? radius { get; set; }
    public string zoom_level { get; set; }

    public string extra_props { get; set; } // jsonb
  }
}
