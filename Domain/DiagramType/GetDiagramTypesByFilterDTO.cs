using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DiagramType
{
  public record GetDiagramTypesByFilterDTO
  (string filter, string start_id, bool forward);
}
