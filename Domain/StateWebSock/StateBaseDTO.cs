namespace Domain.StateWebSock
{
  public class StateBaseDTO
  {  
    public string action { get; set; } = default!;
    public object data { get; set; } = default!;
  }
}
