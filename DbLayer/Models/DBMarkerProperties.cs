using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;
using DbLayer.Models;

namespace DbLayer
{
  internal class DBObjExtraProperty
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid id { get; set; }
    public string prop_name { get; set; }
    public string str_val { get; set; }
    public string visual_type { get; set; }
    public Guid owner_id { get; set; }
  }

  internal class MarkerProp : DBObjExtraProperty { }

  internal record DBMarkerProperties: BasePgEntity
  {
    public List<MarkerProp> extra_props { get; set; }
  }
}
