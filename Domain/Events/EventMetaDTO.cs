using System.Collections.Generic;

namespace Domain.Events
{
  public class EventMetaDTO
  {
    public List<ObjExtraPropertyDTO> extra_props { get; set; } = default!;
    public List<ObjExtraPropertyDTO> not_indexed_props { get; set; } = default!;
  }
}
