namespace Domain.OptionsModels
{
  public class KeycloakSettings
  {
    public string RealmName { get; set; } = default!;
    public string BaseAddr { get; set; } = default!;
    public string admin_name { get; set; } = default!;
    public string admin_password { get; set; } = default!;
    public string admin_client_id { get; set; } = default!;
  }
}
