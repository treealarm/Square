namespace Domain
{
  public record BaseMarkerDTO: BaseObjectDTO
  {
    public string? parent_id { get; set; }
    public string? owner_id { get; set; }
    public string? name { get; set; }
  }
}
