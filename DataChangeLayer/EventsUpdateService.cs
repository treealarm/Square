using Domain.Events;
using Domain.ServiceInterfaces;

namespace DataChangeLayer
{
  public class EventsUpdateService: IEventsUpdateService
  {
    private readonly IEventsService _eventsService;
    public EventsUpdateService(IEventsService eventsService)
    {
      _eventsService = eventsService;
    }
    public async Task<long> AddEvents(List<EventDTO> events)
    {
      await _eventsService.InsertManyAsync(events);
      return events.Count;
    }
  }
}
