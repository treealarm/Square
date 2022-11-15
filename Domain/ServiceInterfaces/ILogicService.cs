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
    public Task<StaticLogicDTO> GetAsync(string id);
  }
}
