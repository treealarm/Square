using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Domain.GeoDBDTO
{
  // This class is for leaflet, so Y goes first in array.
  public class Geo2DCoordDTO: List<double>
  {
    [JsonIgnore]
    public double Y 
    {
      get
      {
        return this[0];
      }
      set
      {
        this[0] = value;
      }
    }

    [JsonIgnore]
    public double X
    {
      get
      {
        return this[1];
      }
      set
      {
        this[1] = value;
      }
    }
  }
}
