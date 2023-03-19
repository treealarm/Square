using Dapr.Client;
using Domain.ServiceInterfaces;
using Domain.StateWebSock;
using Microsoft.AspNetCore.Mvc;

namespace RouterMicroService.Controllers
{
  [ApiController]
  [Route("[controller]")]
  public class RouterController : ControllerBase
  {
    private readonly ILogger<RouterController> _logger;
    private IRoutService _routService;
    private ITrackRouter _router;
    private readonly DaprClient _daprClient;
    public RouterController(
      ILogger<RouterController> logger,
      ITrackRouter router,
      IRoutService routService,
      DaprClient daprClient
    )
    {
      _routService = routService;
      _router = router;
      _logger = logger;
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
  }
}