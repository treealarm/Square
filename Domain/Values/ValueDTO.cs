namespace Domain.Values
{
  public record ValueDTO: BaseObjectDTO
  {
    
    public string? owner_id { get; set; }//объект, который владеет этим значением
    public string? name { get; set; }
    public object? value { get; set; }
  }
}
