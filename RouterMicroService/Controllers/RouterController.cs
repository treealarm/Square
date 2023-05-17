using Dapr.Client;
using Domain.GeoDBDTO;
using Domain.ServiceInterfaces;
using Domain.StateWebSock;
using Microsoft.AspNetCore.Mvc;

namespace RouterMicroService.Controllers
{
  [ApiController]
  [Route("[controller]")]
  public class RouterController : ControllerBase
  {
    private readonly DaprClient _daprClient;
    private ITrackRouter _router;
    public RouterController(
      DaprClient daprClient,
      ITrackRouter router
    )
    {
      _router = router;
      _daprClient = daprClient;
    }


    [HttpGet()]
    [Route("GetHello")]
    public async Task<IEnumerable<string>> GetHello()
    {
      var forecasts = await _daprClient.InvokeMethodAsync<IEnumerable<string>>(
            HttpMethod.Get,
            @"leafletalarms",
            @"api/Tracks/GetHello");

      return forecasts;
    }

    [HttpPost]
    [Route("GetRoute")]
    public async Task<ActionResult<List<Geo2DCoordDTO>>> GetRoute(RoutDTO routData)
    {
      var routRet = await _router.GetRoute(routData.InstanceName, routData.Profile, routData.Coordinates);

      return CreatedAtAction(nameof(GetRoute), routRet);
    }
  }
}