using Domain.Integration;
using Domain.ServiceInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataChangeLayer
{
  internal class IntegrationUpdateService: IIntegrationUpdateService
  {
    private readonly IIntegrationServiceInternal _integrationService;

    private IPubService _pub;
    public IntegrationUpdateService(
      IIntegrationServiceInternal integrationService,
      IPubService pub
    )
    {
      _integrationService = integrationService;
      _pub = pub;
    }
    async Task IIntegrationUpdateService.UpdateListAsync(List<IntegrationDTO> obj2UpdateIn)
    {
      await _integrationService.UpdateListAsync(obj2UpdateIn);
    }
    async Task IIntegrationUpdateService.RemoveAsync(List<string> ids)
    {
      await _integrationService.RemoveAsync(ids);
    }
  }
}
