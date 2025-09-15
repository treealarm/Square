using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;


namespace DbLayer.Models
{
  internal class DBLevel
  {
    public Guid id { get; set; }

    public string zoom_level { get; set; }
    public int zoom_min { get; set; }
    public int zoom_max { get; set; }
  }
}
