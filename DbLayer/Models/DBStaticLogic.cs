using Domain;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbLayer.Models
{
  [BsonIgnoreExtraElements]
  public class DBStaticLogic
  {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string id { get; set; }
    public string name { get; set; }
    public string logic { get; set; }
    public List<DBLogicFigureLink> figs { get; set; }

    [BsonIgnoreIfNull]
    public DBObjPropsSearch property_filter { get; set; }
  }
}
