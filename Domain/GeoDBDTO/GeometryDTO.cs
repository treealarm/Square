using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Domain.GeoDBDTO
{
  public class GeometryDTOBase
  {
    public virtual string type { get; set; }
  }
  
  [JsonConverter(typeof(GeometryDTOConverter))]
  public class GeometryDTO: GeometryDTOBase
  {
    protected object _coord;
    protected string _type;
    public override string type 
    { 
      get
      {
        if (!string.IsNullOrEmpty(_type))
        {
          return _type;
        }

        return GetFigureType();
      }
      set
      {
        _type = value;
      }
    }
    public object coord
    {
      get
      {
        return _coord;
      }
      set
      {
        _coord = value;
      }
    }

    public virtual List<Geo2DCoordDTO> GetCoordArray()
    {
      var retval = coord as List<Geo2DCoordDTO>;

      if (retval == null)
      {
        retval = new List<Geo2DCoordDTO>()
        {
          coord as Geo2DCoordDTO
        };
      }

      return retval;
    }

    public virtual Geo2DCoordDTO GetCentroid()
    {
      return null;
    }
    public string GetFigureType()
    {
      if (this is GeometryCircleDTO)
      {
        return "Point";
      }

      if (this is GeometryPolygonDTO)
      {
        return "Polygon";
      }

      if (this is GeometryPolylineDTO)
      {
        return "LineString";
      }

      return string.Empty;
    }

    public virtual string GetJson()
    {
      return JsonSerializer.Serialize(this);
    }
  }

  public class GeometryDTOConverter : JsonConverter<GeometryDTO>
  {
    public override GeometryDTO Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
      string readerString = String.Empty;

      using (var jsonDoc = JsonDocument.ParseValue(ref reader))
      {
        readerString = jsonDoc.RootElement.GetRawText();
      }

      var geoDto = JsonSerializer.Deserialize(readerString, typeof(GeometryDTOBase)) as GeometryDTOBase;

      if (geoDto.type == "Point")
      {
        return JsonSerializer.Deserialize(readerString, typeof(GeometryCircleDTO)) as GeometryDTO;
      }

      if (geoDto.type == "Polygon")
      {
        return JsonSerializer.Deserialize(readerString, typeof(GeometryPolygonDTO)) as GeometryDTO;
      }

      if (geoDto.type == "LineString")
      {
        return JsonSerializer.Deserialize(readerString, typeof(GeometryPolylineDTO)) as GeometryDTO;
      }
      return geoDto as GeometryDTO;
    }

    public override void Write(
        Utf8JsonWriter writer,
        GeometryDTO geometry,
        JsonSerializerOptions options) 
    {
      var s = geometry.GetJson();
      writer.WriteRawValue(s);
    }            
  }
}
