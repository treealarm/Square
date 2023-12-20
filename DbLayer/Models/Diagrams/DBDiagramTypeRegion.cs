using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbLayer.Models.Diagrams
{
  //class to describe bindings. Like rack position.
  internal class DBDiagramTypeRegion
  {
    public string id { get; set; }
    public DBDiagramCoord geometry {  get; set; }
  }
}
