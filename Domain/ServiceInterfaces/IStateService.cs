using Domain.States;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.ServiceInterfaces
{
  public interface IStateService
  {
    Task Init();
    public Task<long> UpdateStatesAsync(List<ObjectStateDTO> newObjs);
    public Task<long> UpdateStateDescrsAsync(List<ObjectStateDescriptionDTO> newObjs);
    public Task<List<ObjectStateDTO>> GetStatesAsync(List<string> ids);
    public Task<List<ObjectStateDescriptionDTO>> GetStateDescrAsync(
      string external_type,
      List<string> states
    );
    public Task UpdateAlarmStatesAsync(List<AlarmState> alarms);
    public Task<Dictionary<string, AlarmState>> GetAlarmStatesAsync(List<string> ids);
    public Task DropStateAlarms();
  }
}
