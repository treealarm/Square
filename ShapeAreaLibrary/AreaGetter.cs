using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShapeAreaLibrary
{
  public static class AreaGetter
  {
    public static double GetShapeArea(object o)
    {
      var result = o switch
      {
        Circle c => c.Area,
        Triangle t => t.Area,
        _ => throw new ArgumentException("Not recognized shape")
      };
      return result;
    }
  }
}
