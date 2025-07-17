
using System.Collections.Generic;

namespace Domain
{
  public class SearchEventFilterDTO: SearchFilterDTO
  {
    public List<string> groups {  get; set; } = new List<string>();
    public List<string> objects { get; set; } = new List<string>();
    public bool images_only { get; set; } = false;
    public string? param0 { get; set; }
    public string? param1 { get; set; }
  }
}
