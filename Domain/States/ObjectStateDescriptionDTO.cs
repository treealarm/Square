namespace Domain.States
{
  public class ObjectStateDescriptionDTO
  {
    public string id { get; set; } = default!;
    public bool alarm { get; set; } = default!;
    public string state { get; set; } = default!;
    public string state_descr { get; set; } = default!;
    public string state_color { get; set; } = default!;
    public string external_type { get; set; } = default!;
  }
}
