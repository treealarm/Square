
namespace DbLayer.Models
{
  internal record DBDiagram : BasePgEntity
  {
    public string dgr_type { get; set; }
    public DBDiagramCoord geometry { get; set; }
    public string region_id { get; set; }
    public string background_img { get; set; }    
  }
}
