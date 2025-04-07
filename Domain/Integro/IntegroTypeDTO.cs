

using System.Collections.Generic;

namespace Domain
{
  public record IntegroTypeKeyDTO
  {
    public string? i_type { get; set; }
    public string? i_name { get; set; }
  }

  public record IntegroTypeDTO: IntegroTypeKeyDTO
  {
    public List<IntegroTypeChildDTO> children { get; set; } = new List<IntegroTypeChildDTO>();
  }
  // Описываем какой дочерний тип может создать данный тип.
  public record IntegroTypeChildDTO
  {
    public string? child_i_type { get; set; }
  }
}
