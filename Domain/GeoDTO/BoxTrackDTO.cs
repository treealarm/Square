using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.GeoDTO
{
  public class BoxTrackDTO: BoxDTO
  {
    public DateTime? time_start { get; set; }
    public DateTime? time_end { get; set; }
  }
}
