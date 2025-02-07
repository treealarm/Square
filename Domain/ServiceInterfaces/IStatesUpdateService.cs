
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain
{
  public interface IStatesUpdateService
  {
    public Task<bool> UpdateStates(List<ObjectStateDTO> newObjs);
    public Task<long> UpdateStateDescrs(List<ObjectStateDescriptionDTO> newObjs);
    public Task UpdateAlarmStatesAsync(List<AlarmState> alarms);
    public Task DropStateAlarms();
  }
}
