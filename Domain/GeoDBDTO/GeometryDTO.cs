﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Domain.GeoDBDTO
{
  [JsonConverter(typeof(GeometryDTOConverter))]
  public class GeometryDTO
  {
    public string figure_type { get; set; }
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
      return JsonSerializer.Deserialize(reader.GetString(), this.GetType()) as GeometryDTO;
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