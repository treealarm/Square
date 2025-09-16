using System;

namespace DbLayer
{
  internal class DBMarker
  {
    public Guid id { get; set; }

    public Guid? parent_id { get; set; }
    public Guid? owner_id { get; set; }// points to object which receive and own all states

    public string name { get; set; }
  }
}
