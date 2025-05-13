
using System.Collections.Generic;

namespace Domain
{
  public record CredentialDTO
  {
    public string username { get; set; } = string.Empty;
    public string password { get; set; } = string.Empty;
  }
  public record CredentialListDTO
  {
    public List<CredentialDTO>? credentials { get; set; }
  }
}
