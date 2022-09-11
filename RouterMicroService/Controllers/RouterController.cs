using Dapr.Client;
using DbLayer;
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
    public RouterController(
      ILogger<RouterController> logger,
      ITrackRouter router,
      IRoutService routService
    )
    {
      _routService = routService;
      _router = router;
      _logger = logger;
    }

    [HttpGet(Name = "GetRouts")]
    public async Task<IEnumerable<RoutLineDTO>> Get()
    {
      return await _routService.GetAsync(10);
    }
  }
}