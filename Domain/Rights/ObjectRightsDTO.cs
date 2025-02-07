using System.Collections.Generic;

namespace Domain
{
  public class ObjectRightsDTO
  {
    public string? id { get; set; }
    public List<ObjectRightValueDTO>? rights { get; set; }
  }
}
