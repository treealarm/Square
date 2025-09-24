
using System;

namespace DbLayer.Models
{
  internal record DBGroup : BasePgEntity
  {

    public Guid objid { get; set; }
    public string name { get; set; }
  }
}
