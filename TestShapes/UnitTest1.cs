using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShapeAreaLibrary;
using System;

namespace TestShapes
{
  [TestClass]
  public class UnitTest1
  {
    [TestMethod]
    public void TestMethod1()
    {
      IShape shape = new Circle()
      {
        Radius = 10
      };
      var area = shape.Area;
      Assert.IsTrue(Math.Abs(area - Math.PI * 100) < 0.001);

      shape = new Triangle()
      {
        side = new double[] {5,3,4}
      };
      area = shape.Area;
      Assert.IsTrue(Math.Abs(area - 6) < 0.001);
      if(shape is ITypeChecker typeChecker)
      {
        var shapeType = typeChecker.GetShapeType();
        Assert.IsTrue(shapeType == (int)Triangle.TriangleType.rectangular);
      }

      var area2 = AreaGetter.GetShapeArea(shape);
      Assert.IsTrue(Math.Abs(area - area2) < 0.001);
    }
  }
}
