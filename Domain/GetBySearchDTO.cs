using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
  public class GetBySearchDTO
  {
    public List<BaseMarkerDTO> list { get; set; }
    public string search_id { get; set; }
    public string start_id { get; set; }
    public string end_id { get; set; }
  }
}
