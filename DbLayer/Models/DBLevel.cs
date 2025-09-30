
namespace DbLayer.Models
{
  internal record DBLevel : BasePgEntity
  {
    public string zoom_level { get; set; }
    public int zoom_min { get; set; }
    public int zoom_max { get; set; }
  }
}
