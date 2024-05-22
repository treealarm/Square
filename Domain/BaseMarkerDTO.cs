namespace Domain
{
  public record BaseMarkerDTO
  {
    public string id { get; set; } = default!;
    public string parent_id { get; set; } = default!;
    public string name { get; set; } = default!;
    public string external_type { get; set; } = default!;
  }
}
