using Domain.ServiceInterfaces;
using System.Collections.Generic;
using System.Linq;

namespace LeafletAlarms.Services
{
  public class StateQueueForUpdate : IIdsQueue
  {
    private HashSet<string> m_idsToCheck = new HashSet<string>();
    private object _locker = new object();
    List<string> IIdsQueue.GetIds()
    {
      lock (_locker)
      {
        var list = m_idsToCheck.ToList();
        m_idsToCheck.Clear();
        return list;
      }
    }

    void IIdsQueue.AddIds(List<string> objIds)
    {
      lock (_locker)
      {
        foreach (var id in objIds)
        {
          m_idsToCheck.Add(id);
        }
      }
    }

    void IIdsQueue.AddId(string objId)
    {
      lock (_locker)
      {
        m_idsToCheck.Add(objId);
      }
    }
  }
}
