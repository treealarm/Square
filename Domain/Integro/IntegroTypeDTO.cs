

using System.Collections.Generic;

namespace Domain
{
  public record IntegroTypeDTO
  {
    public string? i_type { get; set; }
    public List<IntegroTypeChildDTO> children { get; set; } = new List<IntegroTypeChildDTO>();
  }
  // Описываем какой дочерний тип может создать данный тип.
  public record IntegroTypeChildDTO
  {
    public string? child_i_type { get; set; }
  }
}
