using Domain.Integration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.ServiceInterfaces
{
  public interface IIntegrationUpdateService
  {
    public Task UpdateListAsync(List<IntegrationDTO> obj2UpdateIn);
    public Task RemoveAsync(List<string> ids);
  }
}
