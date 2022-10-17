
using System.Collections.Generic;

namespace Domain
{
  public class BoxDTO
  {
    public double[] wn { get; set; }
    public double[] es { get; set; }
    public double? zoom { get; set; }
    public List<string> ids { get; set; }
    public int? count { get; set; }
  }
}
