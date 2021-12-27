using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShapeAreaLibrary
{
  public class Circle: IShape
  {
    public double Radius
    { get; set; } = 0;
    
    public virtual double Area
    {
      get
      {
        return Math.PI * Radius * Radius;
      }      
    }
  }
}
