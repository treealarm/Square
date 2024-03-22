using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
  public class SearchFilterDTO
  {
    public DateTime? time_start { get; set; }
    public DateTime? time_end { get; set; }
    public ObjPropsSearchDTO property_filter { get; set; }
    public string search_id { get; set; }
    public string start_id { get; set; }
    public bool forward { get; set; }
    public int count { get; set; }
    public Dictionary<string, string> sort { get; set; }
  }
}
