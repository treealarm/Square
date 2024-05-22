using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
  public class ObjExtraPropertyDTO: KeyValueDTO
  {
    public string visual_type { get; set; } = default!;
  }
}
