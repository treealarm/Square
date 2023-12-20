using DbLayer.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbLayer
{
  [BsonIgnoreExtraElements]
  internal class DBTrackPoint
  {
    public DBFigTsMeta meta { get; set; } = new DBFigTsMeta();
    public DateTime timestamp { get; set; }
  }
}
