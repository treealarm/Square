using Domain.GeoDBDTO;
using System.Collections.Generic;


namespace TrackSender.Models
{
  // Root myDeserializedClass = JsonConvert.DeserializeObject<List<Root>>(myJsonResponse);
  public class Geojson
  {
    public string type { get; set; }
    public dynamic coordinates { get; set; }
  }


  public class CoordinateList : List<Geo2DCoordDTO>
  {
  }

  public class MultiPolygon: List<List<List<Geo2DCoordDTO>>>
  {

  }
  public class Root
  {   
    public int place_id { get; set; }
    public string licence { get; set; }
    public string osm_type { get; set; }
    public int osm_id { get; set; }
    public List<string> boundingbox { get; set; }
    public string lat { get; set; }
    public string lon { get; set; }
    public string display_name { get; set; }
    public string @class { get; set; }
    public string type { get; set; }
    public double importance { get; set; }
    public string icon { get; set; }
    public Geojson geojson { get; set; }
  }
}
