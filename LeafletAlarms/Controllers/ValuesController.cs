using Domain.ServiceInterfaces;
using Domain.Values;
using Microsoft.AspNetCore.Mvc;
using System.Xml.Linq;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LeafletAlarms.Controllers
{

  [Route("api/[controller]")]
  [ApiController]
  public class ValuesController : ControllerBase  
  {
    private readonly IValuesService _valuesService;
    private readonly IValuesUpdateService _valuesUpdateService;
    public ValuesController(
     IValuesService valuesService,
     IValuesUpdateService valuesUpdateService
    )
    {
      _valuesService = valuesService;
      _valuesUpdateService = valuesUpdateService;
    }

    static ValueDTO _value = new ValueDTO()
    {
        name = "test",
        owner_id = "671f6f6f593be419bbece727"
    };

    [HttpGet()]
    [Route("GetByOwner")]
    public async Task<List<ValueDTO>> GetByOwner(string owner)
    {
      var dic = await _valuesService.GetListByOwnerAsync(owner);
      return dic.Values.ToList();
    }
    [HttpGet()]
    [Route("GetById")]
    public async Task<List<ValueDTO>> GetById(string id)
    {
      var dic = await _valuesService.GetListByIdsAsync(new List<string> { id });
      return dic.Values.ToList();
    }

    [HttpPost]
    public void Post([FromBody] List<ValueDTO> values)
    {
      _valuesUpdateService.UpdateValues(values);
    }

    [HttpDelete()]
    public async Task Delete(List<string> ids)
    {
      await _valuesUpdateService.RemoveValues(ids);
    }
  }
}
