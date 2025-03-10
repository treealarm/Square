﻿using Domain;
using Microsoft.AspNetCore.Mvc;

namespace LeafletAlarms.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class EventsController : ControllerBase
  {
    private readonly IEventsService _eventsService;
    private readonly IEventsUpdateService _eventsUpdateService;
    
    public EventsController(
      IEventsService eventsService, 
      IEventsUpdateService eventsUpdateService)
    {
      _eventsService = eventsService;
      _eventsUpdateService = eventsUpdateService;
    }

    [HttpPost]
    [Route("GetByFilter")]
    public async Task<List<EventDTO>> GetByFilter(
      SearchEventFilterDTO filter
    )
    {
      var retVal =  await _eventsService.GetEventsByFilter(filter);
      return retVal;
    }


    [HttpPost()]
    [Route("AddEvents")]
    public async Task<long> AddEvents(List<EventDTO> events)
    {
      return await _eventsUpdateService.AddEvents(events);
    }
  }
}
