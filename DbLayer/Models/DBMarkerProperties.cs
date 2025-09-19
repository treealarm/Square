using System;
using DbLayer.Models;

namespace DbLayer
{
  internal record DBMarkerProp : BasePgEntity
  {
    public string prop_name { get; set; }
    public string str_val { get; set; }
    public string visual_type { get; set; }
    public Guid object_id { get; set; }
  }

  internal record DBObjExtraProperty : BasePgEntity//TODO remove
  {
    public string prop_name { get; set; }
    public string str_val { get; set; }
    public string visual_type { get; set; }
    public Guid owner_id { get; set; }
  }
}
