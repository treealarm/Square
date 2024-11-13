using Domain.DiagramType;

namespace Domain.Diagram
{
  public record DiagramFullDTO
  {
    public required DiagramDTO diagram { get; set; }
    public DiagramTypeDTO? parent_type { get; set; }
  }
}
