using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
  public class LogicFigureLinkDTO
  {
    public string id { get; set; }
    public string group_id { get; set; }
  }

  public class StaticLogicDTO
  {
    public string id { get; set; }
    public string name { get; set; }
    public string logic { get; set; }
    public List<LogicFigureLinkDTO> figs { get; set; }
  }
}
