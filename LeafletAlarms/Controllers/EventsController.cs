using Domain;
using Domain.Events;
using Domain.ServiceInterfaces;
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

    [HttpPost]
    [Route("GetByFilter")]
    public async Task<List<EventDTO>> GetByFilter(
      SearchFilterDTO filter
    )
    {
      return await _eventsService.GetEventsByFilter(filter);
    }

    [HttpGet]
    [Route("ReserveCursor")]
    public async Task<long> ReserveCursor(string search_id)
    {
      return await _eventsService.ReserveCursor(search_id);
    }
  }
}
