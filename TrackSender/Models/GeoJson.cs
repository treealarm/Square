using Domain.GeoDBDTO;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Xml.Linq;

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

  public class Polygon : List<CoordinateList>
  {

  }
  public class MultiPolygon: List<Polygon>
  {

  }
  
  public class Centroid
  {
    public string type { get; set; }
    public List<double> coordinates { get; set; }
  }

  public class Extratags
  {
    [JsonPropertyName("contact:website")]
    public string ContactWebsite { get; set; }
    public string wikidata { get; set; }
    public string wikipedia { get; set; }
  }

  public class Geometry
  {
    public string type { get; set; }
    public dynamic coordinates { get; set; }
  }

  public class Names
  {
    public string name { get; set; }

    [JsonPropertyName("name:be")]
    public string NameBe { get; set; }

    [JsonPropertyName("name:ca")]
    public string NameCa { get; set; }

    [JsonPropertyName("name:de")]
    public string NameDe { get; set; }

    [JsonPropertyName("name:en")]
    public string NameEn { get; set; }

    [JsonPropertyName("name:es")]
    public string NameEs { get; set; }

    [JsonPropertyName("name:ru")]
    public string NameRu { get; set; }
    public string @ref { get; set; }
  }

  public class Root
  {
    public int place_id { get; set; }
    public int parent_place_id { get; set; }
    public string osm_type { get; set; }
    public int osm_id { get; set; }
    public string category { get; set; }
    public string type { get; set; }
    public int admin_level { get; set; }
    public string localname { get; set; }

    [JsonIgnore]
    public Names names
    { 
      get
      {
        try
        {
          var names_temp = _names as Names;

          if (names_temp == null)
          {
            return new Names();
          }

          return names_temp;
        }
        catch (Exception) { }

        return new Names();
      }
    }

    [JsonPropertyName("names")]
    public object _names { get; set; }
    public object addresstags { get; set; }
    public object housenumber { get; set; }
    public object calculated_postcode { get; set; }
    public string country_code { get; set; }
    public DateTime indexed_date { get; set; }
    public double importance { get; set; }
    public double calculated_importance { get; set; }
    public Extratags extratags { get; set; }
    public string calculated_wikipedia { get; set; }
    public string icon { get; set; }
    public int rank_address { get; set; }
    public int rank_search { get; set; }
    public bool isarea { get; set; }
    public Centroid centroid { get; set; }
    public Geometry geometry { get; set; }
  }


  public class Root1
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
