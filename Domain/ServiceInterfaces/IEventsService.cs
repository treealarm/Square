
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain
{
  public interface IEventsService
  {
    Task<long> InsertManyAsync(List<EventDTO> newObjs);
    Task<List<EventDTO>> GetEventsByFilter(SearchEventFilterDTO filter_in);
  }
}
