using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Domain
{
  public enum Order
  {
    asc,
    desc  
  }

  public class SortDTO
  {
    public string key { get; set; } = default!;
    public string order { get; set; } = default!;
  }

  public class SearchFilterDTO
  {
    public DateTime? time_start { get; set; }
    public DateTime? time_end { get; set; }
    public ObjPropsSearchDTO property_filter { get; set; } = default!;
    public string search_id { get; set; } = default!;
    public string start_id { get; set; } = default!;
    public int forward { get; set; }
    public int count { get; set; }
    public List<SortDTO> sort { get; set; } = default!;
    public override int GetHashCode()
    {
      var hash = time_start?.GetHashCode() ^
                time_end?.GetHashCode() ^
                search_id?.GetHashCode() ^
                start_id?.GetHashCode() ^
                count.GetHashCode();
       
      if (sort != null)
      {
        foreach (var item in sort)
        {
          hash = hash ^ item.key.GetHashCode() ^ item.order.GetHashCode();
        }
      }
      
      if (property_filter != null && property_filter.props != null)
      {
        foreach (var item in property_filter.props)
        {
          hash = hash ^ item.prop_name.GetHashCode() ^ item.str_val.GetHashCode();
        }
      }
      
      if (hash != null)
      {
        return hash.Value;
      }
      return 0;
    }
  }
}
