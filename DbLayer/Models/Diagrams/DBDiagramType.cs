using System.Collections.Generic;
using System;


namespace DbLayer.Models
{
  internal record DBDiagramTypeRegion : BasePgEntity
  {
    public Guid diagram_type_id { get; set; }
    public DBDiagramType diagram_type { get; set; }
    public string region_key { get; set; }
    public DBDiagramCoord geometry { get; set; }
    public Dictionary<string, string> styles { get; set; }
  }

  internal record DBDiagramType : BasePgEntity
  {
    public string name { get; set; }
    public string src { get; set; }
    public List<DBDiagramTypeRegion> regions { get; set; }
  }
}
