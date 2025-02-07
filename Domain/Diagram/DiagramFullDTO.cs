using Domain;

namespace Domain
{
  public record DiagramFullDTO
  {
    public required DiagramDTO diagram { get; set; }
    public DiagramTypeDTO? parent_type { get; set; }
  }
}
