using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
  public class ObjPropsDTO : FigureBaseDTO
  {
    public dynamic geometry { get; set; }
    public List<ObjExtraPropertyDTO> extra_props { get; set; } = 
      new List<ObjExtraPropertyDTO>() { 
        new ObjExtraPropertyDTO () { str_val = DateTime.Now.ToString() },
        new ObjExtraPropertyDTO() { str_val = "test2" } };
  }
}
