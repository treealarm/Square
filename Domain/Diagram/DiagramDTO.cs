namespace Domain.Diagram
{
  public record DiagramDTO
  {
    public string? id { get; set; }
    public DiagramCoordDTO? geometry { get; set; }
    public string? region_id { get; set; }
    public string? dgr_type { get; set; }
    public string? background_img { get; set; }
  }
}
