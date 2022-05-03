using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
  public class GetByParentDTO
  {
    public List<MarkerDTO> children { get; set; }
    public List<TreeMarkerDTO> parents { get; set; }
    public string parent_id { get; set; }
  }
}
