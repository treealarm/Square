using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.OptionsModels
{
  public class KeycloakSettings
  {
    public string RealmName { get; set; }
    public string PublicKeyJWT { get; set; }
    public string BaseAddr { get; set; }
    public string admin_name { get; set; }
    public string admin_password { get; set; }
    public string admin_client_id { get; set; }
  }
}
