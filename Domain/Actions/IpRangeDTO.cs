
namespace Domain
{ 
  public record IpRangeDTO
  {
    public string start_ip { get; set; } = string.Empty;
    public string end_ip { get; set; } = string.Empty;
  }
}
