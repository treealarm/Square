using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Logic
{
  public class LogicTriggered
  {
    public string LogicId { get; set; }
    public HashSet<string> LogicTextObjects { get; set; }
    public int Count { get; set; }
  }
}
