using Domain.Integro;
using Domain.ServiceInterfaces;
using Domain.Values;
using Microsoft.AspNetCore.Mvc;

namespace LeafletAlarms.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class IntegroController : ControllerBase
  {
    private readonly IMapService _mapService;
    private readonly IIntegroUpdateService _integroUpdateService;
    private readonly IIntegroService _integroService;

    public IntegroController(
      IMapService mapService,
      IIntegroUpdateService integroUpdateService,
      IIntegroService integroService
    )
    {
      _mapService = mapService;
      _integroUpdateService = integroUpdateService;
      _integroService = integroService;
    }

    [HttpPost()]
    [Route("GetByIds")]
    public async Task<List<IntegroDTO>> GetByIds(List<string> ids)
    {
      var dic = await _integroService.GetListByIdsAsync(ids);
      return dic.Values.ToList();
    }

    [HttpPost]
    [Route("UpdateIntegros")]
    public async Task<List<IntegroDTO>> UpdateValues([FromBody] List<IntegroDTO> integros)
    {
      await _integroUpdateService.UpdateIntegros(integros);
      return integros;
    }

    [HttpDelete()]
    [Route("DeleteIntegros")]
    public async Task DeleteValues(List<string> ids)
    {
      await _integroUpdateService.RemoveIntegros(ids);
    }
  }
}
