namespace Domain.StateWebSock
{
  public class StateBaseDTO
  {  
    public string? action { get; set; }
    public object? data { get; set; }
  }

  public class StateBaseReceiveDTO
  {
    public string? action { get; set; }
    public object? data { get; set; }
  }
}
