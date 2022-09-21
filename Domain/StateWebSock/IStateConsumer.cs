using Domain.States;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.StateWebSock
{
  public interface IStateConsumer
  {
    Task OnStateChanged(List<ObjectStateDTO> state);
    Task OnBlinkStateChanged(List<AlarmObject> blinkStates);
  }
}
