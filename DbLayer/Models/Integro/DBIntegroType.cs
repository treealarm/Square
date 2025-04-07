using System.Collections.Generic;

namespace DbLayer.Models
{
  internal record DBIntegroType
  {
    public string i_type { get; set; }
    public string i_name { get; set; }
    public List<DBIntegroTypeChild> children { get; set; } = new();
  }

  internal record DBIntegroTypeChild
  {
    public string i_type { get; set; }
    public string i_name { get; set; }
    public string child_i_type { get; set; }
  }
}
