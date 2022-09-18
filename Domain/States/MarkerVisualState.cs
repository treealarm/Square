using Domain.States;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.StateWebSock
{
  public class MarkersVisualStatesDTO
  {
    public List<ObjectStateDTO> states { get; set; }
    public List<ObjectStateDescriptionDTO> states_descr { get; set; }
  }
}
