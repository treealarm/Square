using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.ObjectInterfaces
{
  public interface IObjectZoom
  {
    public int? zoom_min { get; set; }
    public int? zoom_max { get; set; }
  }
}
