using System.Text.Json;
using System;

namespace DbLayer.Models
{
  internal record DBValue : BasePgEntity
  {
    public Guid owner_id { get; set; }

    public string name { get; set; }

    public JsonDocument value { get; set; }
  }
}
