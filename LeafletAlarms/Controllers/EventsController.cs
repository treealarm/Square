using DbLayer.Services;
using Domain.Diagram;
using Domain.Events;
using Domain.ServiceInterfaces;
using LeafletAlarms.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LeafletAlarms.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class EventsController : ControllerBase
  {
    private readonly IEventsService _eventsService;
    public EventsController(IEventsService eventsService)
    {
      _eventsService = eventsService;
    }

    [HttpPost()]
    [Route("AddEvents")]
    public async Task<long> AddEvents(List<EventDTO> events)
    {
      await _eventsService.InsertManyAsync(events);
      return events.Count;
    }
  }
}
