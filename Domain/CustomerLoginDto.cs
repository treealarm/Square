namespace Domain
{
  public record CustomerLoginDto
  {
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
  }
  public class RefreshTokenRequest
  {
    public string refresh_token { get; set; } = string.Empty;
  }
  public class Constants
  {
    public const string PubClient = "pubclient";
  }
}
