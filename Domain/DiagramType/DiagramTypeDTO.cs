using System.Collections.Generic;

namespace Domain.DiagramType
{
  public record DiagramTypeDTO
  {
    public string id { get; set; } = default!;
    public string name { get; set; } = default!;
    public string src { get; set; } = default!;
    public List<DiagramTypeRegionDTO> regions { get; set; } = default!;
  }
}
