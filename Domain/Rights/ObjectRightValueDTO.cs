using System;

namespace Domain
{
  public class ObjectRightValueDTO
  {
    [Flags]
    public enum ERightValue: int
    {
      None = 0,
      View = 1,
      Control = 2
      //,Delete = 4
      //,Add = 8
    }
    public string? role { get; set; }
    public ERightValue value { get; set; }
  }
}
