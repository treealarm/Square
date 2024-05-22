using System;
using System.Collections.Generic;

namespace Domain.States
{
  public class ObjectStateDTO
  {
    public string id { get; set; } = default!;
    public List<string> states { get; set; } = default!;
    public DateTime timestamp { get; set; } = default!;
  }
}
