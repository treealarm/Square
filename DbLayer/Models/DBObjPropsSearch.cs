using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbLayer.Models
{
  public class DBObjPropsSearch
  {
    [BsonIgnoreIfNull]
    public List<DBObjExtraProperty> props { get; set; }
  }
}
