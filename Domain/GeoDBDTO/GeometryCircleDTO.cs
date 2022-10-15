using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Domain.GeoDBDTO
{
  public class GeometryCircleDTO : GeometryDTO
  {
    [JsonConstructor]
    public GeometryCircleDTO() { }
    public GeometryCircleDTO(Geo2DCoordDTO crd)
    {
      this.coord = crd;
    }
    public new Geo2DCoordDTO coord 
    { 
      get
      {
        return _coord as Geo2DCoordDTO;
      }
      set
      {
        _coord = value;
      }
    }

    public override string GetJson()
    {
      return JsonSerializer.Serialize(this);
    }
  }
}
