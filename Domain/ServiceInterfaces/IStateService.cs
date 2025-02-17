﻿using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain
{
  public interface IStateService
  {
    Task Init();
    public Task<long> UpdateStatesAsync(List<ObjectStateDTO> newObjs);
    public Task<long> UpdateStateDescrsAsync(List<ObjectStateDescriptionDTO> newObjs);
    public Task<Dictionary<string, ObjectStateDTO>> GetStatesAsync(List<string> ids);
    public Task<List<ObjectStateDescriptionDTO>> GetStateDescrAsync(
      List<string> states
    );
    public Task UpdateAlarmStatesAsync(List<AlarmState> alarms);
    public Task<Dictionary<string, AlarmState>> GetAlarmStatesAsync(List<string> ids);
    public Task DropStateAlarms();
    public Task<Dictionary<string, ObjectStateDTO>> GetAlarmedStates(List<string> statesFilter);
    public Task<Dictionary<string, ObjectStateDescriptionDTO>> GetAlarmStatesDescr(List<string> statesFilter);
  }
}
