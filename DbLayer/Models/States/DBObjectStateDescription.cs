namespace DbLayer.Models
{
  internal record DBObjectStateDescription : BasePgEntity
  {
    public bool alarm { get; set; }
    public string state { get; set; }
    public string state_descr { get; set; }
    public string state_color { get; set; }
  }
}
