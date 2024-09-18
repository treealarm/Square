using System.Collections.Generic;

namespace Domain.Integration
{
  public record GetIntegrationLeafsDTO
  {
    public string? integration_id { get; set; }
    public List<IntegrationLeafDTO>? children { get; set; }
  }
}
