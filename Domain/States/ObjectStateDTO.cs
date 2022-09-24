using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.States
{
  public class ObjectStateDTO
  {
    public string id { get; set; }
    public List<string> states { get; set; }
    public DateTime timestamp { get; set; } = DateTime.UtcNow;
  }
}
