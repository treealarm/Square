using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShapeAreaLibrary
{
  public class Triangle : IShape, ITypeChecker
  {
    public enum TriangleType
    {
      usual,
      rectangular
    }
    public double[] side { get; set; } = new double[3];
    public double b { get; set; }
    public double c { get; set; }
    public virtual double Area
    {
      get
      {
        //S = √p(p - a)(p - b)(p - c)
        var p = (side[0] + side[1] + side[2]) /2;
        var s = Math.Sqrt(p * (p - side[0]) * (p - side[1]) * (p - side[2]));
        return s;
      }
    }

    int ITypeChecker.GetShapeType()
    {
      Array.Sort(side);
      var test = side[0] * side[0] + side[1] * side[1] - side[2] * side[2];
      if(Math.Abs(test) < 0.0000001)
      {
        return (int)TriangleType.rectangular;
      }
      return (int)TriangleType.usual;
    }
  }
}
