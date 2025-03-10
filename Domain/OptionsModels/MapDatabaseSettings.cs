﻿

namespace Domain
{
  public class MapDatabaseSettings
  {
    // We can replace these values from appsettings.json by env. vars.
    // For example MapDatabase__DatabaseName key to replace DatabaseName
    public string ConnectionString { get; set; } = null!;
    public string DatabaseName { get; set; } = null!;
    public string PgHost { get; set; } = null!;    
    public int PgPort { get; set; } = 5432;


    public string ObjectsCollectionName { get; set; } = "Objects";
    public string GeoCollectionName { get; set; } = "Geometry";
    public string TracksCollectionName { get; set; } = "Tracks";
    public string RoutesCollectionName { get; set; } = "Routes";

    public string PropCollectionName { get; set; } = "Properties";
    public string LevelCollectionName { get; set; } = "Levels";
    public string StateCollectionName { get; set; } = "States";
    public string StateDescrCollectionName { get; set; } = "StateDescrs";
    public string StateAlarmsCollectionName { get; set; } = "StateAlarms";
    public string RightsCollectionName { get; set; } = "Rights";
    public string DiagramTypeCollectionName { get; set; } = "DiagramType";
    public string DiagramCollectionName { get; set; } = "Diagram";
    public string EventsCollectionName { get; set; } = "Events";
    public string ValuesCollectionName { get; set; } = "Values";
    public string IntegroCollectionName { get; set; } = "Integro";
    public string GroupsCollectionName { get; set; } = "Groups";

  }
}
