using System.Collections.Generic;

namespace Domain.Integration
{
  public record GetIntegrationLeafsDTO
  {
    public string? parent_id { get; set; }
    public List<IntegrationLeafsDTO>? children { get; set; }
  }
}
