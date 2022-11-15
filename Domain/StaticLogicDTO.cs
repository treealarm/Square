using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
  public class StaticLogicDTO
  {
    public string id { get; set; }
    public string logic { get; set; }
    public List<List<string>> figs { get; set; }
  }
}
