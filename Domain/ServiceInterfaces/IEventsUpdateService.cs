
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain
{
  public interface IEventsUpdateService
  {
    public Task<long> AddEvents(List<EventDTO> events);
  }
}
