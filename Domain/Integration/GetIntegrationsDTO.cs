using System.Collections.Generic;

namespace Domain.Integration
{
  public record GetIntegrationsDTO
  {
    public string? parent_id { get; set; }
    public List<IntegrationExDTO>? children { get; set; }
  }
}
