using GeoJSON.Net;
using GeoJSON.Net.Geometry;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPKGConvertor
{
  public class GeometryHolder
  {
    public string id { get; set; }
    public JObject geom { get; set; }
  }
}
