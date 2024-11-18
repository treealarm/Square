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
    static ValueDTO _value = new ValueDTO()
    {
        name = "test",
        owner_id = "671f6f6f593be419bbece727"
      };
    // GET api/<ValuesController>/5
    [HttpGet("{id}")]
    public ValueDTO Get(string id)
    {
      return _value;
    }

    // POST api/<ValuesController>
    [HttpPost]
    public void Post([FromBody] ValueDTO value)
    {
      _value = value;
    }


    // DELETE api/<ValuesController>/5
    [HttpDelete("{id}")]
    public void Delete(string id)
    {
    }
  }
}
