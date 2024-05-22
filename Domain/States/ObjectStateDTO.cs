using System;
using System.Collections.Generic;

namespace Domain.States
{
  public class ObjectStateDTO
  {
    public string? id { get; set; }
    public List<string>? states { get; set; }
    public DateTime? timestamp { get; set; }
  }
}
