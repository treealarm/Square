

namespace DbLayer.Models
{
  //class to describe bindings. Like rack position.
  internal class DBDiagramTypeRegion
  {
    public string id { get; set; }
    public DBDiagramCoord geometry {  get; set; }
  }
}
