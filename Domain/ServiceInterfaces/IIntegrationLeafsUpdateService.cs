﻿using Domain.Integration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.ServiceInterfaces
{
  public interface IIntegrationLeafsUpdateService
  {
    public Task UpdateListAsync(List<IntegrationLeafDTO> obj2UpdateIn);
    public Task RemoveAsync(List<string> ids);
  }
}
