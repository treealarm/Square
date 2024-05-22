using System.Collections.Generic;

namespace Domain.Rights
{
  public class ObjectRightsDTO
  {
    public string? id { get; set; }
    public List<ObjectRightValueDTO>? rights { get; set; }
  }
}
