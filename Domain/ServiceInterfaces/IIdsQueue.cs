using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.ServiceInterfaces
{
  public interface IIdsQueue
  {
    void AddIds(List<string> objIds);
    void AddId(string objIds);
    List<string> GetIds();
  }
}
