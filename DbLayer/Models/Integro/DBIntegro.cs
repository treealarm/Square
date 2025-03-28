namespace DbLayer.Models
{
  internal record DBIntegro : BasePgEntity
  {
    public string i_name { get; set; }
    public string i_type { get; set; }
  }
}
