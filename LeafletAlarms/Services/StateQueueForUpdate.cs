using Domain.ServiceInterfaces;
using PubSubLib;
using System.Collections.Generic;
using System.Linq;

namespace LeafletAlarms.Services
{
  public class StateQueueForUpdate : ConcurentHashSet
  {
  }
}
