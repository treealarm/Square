using Domain.GeoDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.ServiceInterfaces
{
  public interface ILogicProcessorService
  {
    public Task<List<string>> GetLogicByFigure(GeoObjectDTO figure);
    public Task Drop();
    public Task Insert(GeoObjectDTO figure, string logic_id);
  }
}
