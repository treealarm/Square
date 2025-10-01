

namespace Domain
{
  public class MapDatabaseSettings
  {
    // We can replace these values from appsettings.json by env. vars.
    // For example MapDatabase__DatabaseName key to replace DatabaseName
    public string DatabaseName { get; set; } = null!;
    public string PgHost { get; set; } = null!;    
    public int PgPort { get; set; } = 5432;
  }
}
