using Domain.GeoDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.StateWebSock
{
  public class RoutLineDTO
  {
    public enum EntityType
    {
      not_processed,
      processsed
    }
    public string id { get; set; }
    public GeoObjectDTO figure { get; set; }
    public string id_start { get; set; }
    public string id_end { get; set; }
    public DateTime? ts_start { get; set; }
    public DateTime? ts_end { get; set; }
    public EntityType processed { get; set; }
  }
}
