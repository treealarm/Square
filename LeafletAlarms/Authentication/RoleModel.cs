namespace LeafletAlarms.Authentication
{
  public class RoleModel
  {
    public string id { get; set; }
    public string name { get; set; }
    public string description { get; set; }
    public bool composite { get; set; }
    public bool clientRole { get; set; }
    public string containerId { get; set; }
  }
}
