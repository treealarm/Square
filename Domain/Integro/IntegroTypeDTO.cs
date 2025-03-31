

using System.Collections.Generic;

namespace Domain
{
  public record IntegroTypeDTO
  {
    public string? i_type { get; set; }
    public List<IntegroTypeChildDTO> children { get; set; } = new List<IntegroTypeChildDTO>();
  }

  public record IntegroTypeChildDTO
  {
    public string? i_type { get; set; }
    public string? child_i_type { get; set; }
  }
}
