using Domain.Events;
using Domain.StateWebSock;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.ServiceInterfaces
{
  public interface IEventsService
  {
    Task<long> InsertManyAsync(List<EventDTO> newObjs);
  }
}
