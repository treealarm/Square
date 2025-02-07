using System;
using System.Collections.Generic;

namespace Domain
{
  public class ObjectStateDTO
  {
    public string? id { get; set; }
    public List<string>? states { get; set; }
    public DateTime? timestamp { get; set; }
  }
}
