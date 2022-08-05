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
    public Geo2DCoordDTO coord { get; set; }

    public override string GetJson()
    {
      return JsonSerializer.Serialize(this);
    }
  }
}
