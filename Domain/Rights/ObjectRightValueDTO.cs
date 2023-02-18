using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Rights
{
  public class ObjectRightValueDTO
  {
    [Flags]
    public enum ERightValue: int
    {
      None,
      View,
      Update,
      Delete,
      Add
    }
    public string role { get; set; }
    public ERightValue value { get; set; }
  }
}
