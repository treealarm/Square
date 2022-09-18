using Domain.States;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.ServiceInterfaces
{
  public interface IStateService
  {
    Task Init();
    public Task InsertStatesAsync(List<ObjectStateDTO> newObjs);
    public Task InsertStateDescrsAsync(List<ObjectStateDescriptionDTO> newObjs);
    public Task<List<ObjectStateDTO>> GetStatesAsync(List<string> ids);
    public Task<List<ObjectStateDescriptionDTO>> GetStateDescrAsync(
      string external_type,
      List<string> states
    );
  }
}
