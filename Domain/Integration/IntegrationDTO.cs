
namespace Domain.Integration
{
  public record IntegrationDTO
  {
    public string? id { get; set; }
    public string? parent_id { get; set; }
    public string? name { get; set; }
  }
  public record IntegrationExDTO: IntegrationDTO
  {
    public bool has_children { get; set; }
  }
}
