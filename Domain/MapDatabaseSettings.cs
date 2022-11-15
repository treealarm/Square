using System;

namespace Domain
{
  public class MapDatabaseSettings
  {
    public string ConnectionString { get; set; } = null!;
    public string DatabaseName { get; set; } = null!;
    public string ObjectsCollectionName { get; set; } = null!;
    public string GeoCollectionName { get; set; } = null!;
    public string TracksCollectionName { get; set; } = null!;
    public string RoutsCollectionName { get; set; } = null!;
    
    public string PropCollectionName { get; set; } = null!;
    public string LevelCollectionName { get; set; } = null!;
    public string StateCollectionName { get; set; } = null!;
    public string StateDescrCollectionName { get; set; } = null!;
    public string LogicCollectionName { get; set; } = null!;
  }
}
