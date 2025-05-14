
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain
{
  public interface IActionsService
  {
    
  }
  public interface IActionsUpdateService
  {
    Task UpdateListAsync(List<ActionExeDTO> actions);
  }
  internal interface IActionsServiceInternal
  {
    Task UpdateListAsync(List<ActionExeDTO> actions);
  }
}
