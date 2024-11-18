namespace Domain.Values
{
  public record ValueDTO
  {
    public string? id { get; set; }
    public required string owner_id { get; set; }
    public required string name { get; set; }
    public object? value { get; set; }
    public object? min { get; set; }
    public object? max { get; set; }
  }
}
