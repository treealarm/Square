using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Rights
{
  public class ObjectRightsDTO
  {
    public string id { get; set; }
    public List<ObjectRightValueDTO> rights { get; set; }
  }
}
