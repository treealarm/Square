using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.ServiceInterfaces
{
  public interface ILogicService
  {
    public Task UpdateAsync(StaticLogicDTO obj2UpdateIn);
    public Task DeleteAsync(string id);
    public Task<List<StaticLogicDTO>> GetListByIdsAsync(List<string> ids);
    public Task<List<StaticLogicDTO>> GetByFigureAsync(string id);
    public Task<List<StaticLogicDTO>> GetByName(string name);
    public Task<List<StaticLogicDTO>> GetListAsync(
      string start_id,
      bool forward,
      int count
    );
  }
}
