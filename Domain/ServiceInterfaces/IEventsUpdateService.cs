using Domain.Events;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.ServiceInterfaces
{
  public interface IEventsUpdateService
  {
    public Task<long> AddEvents(List<EventDTO> events);
  }
}
