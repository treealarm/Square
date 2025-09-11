

using System;

namespace DbLayer.Models
{
  internal class DBObjectRightValue
  {
    public Guid object_id { get; set; }
    public string role { get; set; }
    public int value { get; set; }
  }
}
