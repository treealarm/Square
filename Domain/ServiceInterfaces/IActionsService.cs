
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain
{
  public interface IActionsService
  {
    Task<List<ActionExeInfoDTO>> GetActionsByObjectId(string objectId);
  }
  public interface IActionsUpdateService
  {
    Task UpdateListAsync(List<ActionExeDTO> actions);
    Task UpdateResultsAsync(List<ActionExeResultDTO> results);
  }
  internal interface IActionsServiceInternal
  {
    Task UpdateListAsync(List<ActionExeDTO> actions);
    Task UpdateResultsAsync(List<ActionExeResultDTO> results);
  }
}
