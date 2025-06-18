using System;
using System.Collections.Generic;

namespace DbLayer.Models
{
  internal record DBObjectStateValue : BasePgEntity
  {
    public Guid object_id { get; set; } // внешний ключ на DBObjectState
    public string state { get; set; }

  }
  internal record DBObjectState : BasePgEntity
  {
    public DateTime timestamp { get; set; }

    public List<DBObjectStateValue> states { get; set; } = new();
  }
}
