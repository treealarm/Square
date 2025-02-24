using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;

namespace DbLayer
{
  internal class DBObjExtraProperty
  {
    public string prop_name { get; set; }
    [BsonIgnoreIfNull]
    public BsonValue str_val { get; set; }
    [BsonIgnoreIfNull]
    public string visual_type { get; set; }
  }

  internal class PgDBObjExtraProperty
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid id { get; set; }
    public string prop_name { get; set; }
    public string str_val { get; set; }
    public string visual_type { get; set; }
    public Guid owner_id { get; set; }
  }

  internal class DBMarkerProperties
  {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string id { get; set; }
    [BsonIgnoreIfNull]
    public List<DBObjExtraProperty> extra_props { get; set; }
  }
}
