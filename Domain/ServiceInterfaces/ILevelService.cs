using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.ServiceInterfaces
{
  public interface ILevelService
  {
    Task<List<string>> GetLevelsByZoom(double? zoom);
    Task Init();
  }
}
