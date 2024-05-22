using Domain.States;
using System.Collections.Generic;

namespace Domain.StateWebSock
{
  public class MarkersVisualStatesDTO
  {
    public List<ObjectStateDTO> states { get; set; } = default!;
    public List<ObjectStateDescriptionDTO> states_descr { get; set; } = default!;
    public List<AlarmState> alarmed_objects { get; set; } = default!;
  }
}
