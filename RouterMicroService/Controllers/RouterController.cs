using Dapr.Client;
using Domain.ServiceInterfaces;
using Microsoft.AspNetCore.Mvc;

namespace RouterMicroService.Controllers
{
  [ApiController]
  [Route("[controller]")]
  public class RouterController : ControllerBase
  {    
    private readonly ILogger<RouterController> _logger;

    public RouterController(
      ILogger<RouterController> logger
    )
    {
      _logger = logger;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<WeatherForecast> Get()
    {
      return Enumerable.Range(1, 5).Select(index => new WeatherForecast
      {
        Date = DateTime.Now.AddDays(index),
        TemperatureC = Random.Shared.Next(-20, 55),
        Summary = "Hello world!"
      })
      .ToArray();
    }
  }
}