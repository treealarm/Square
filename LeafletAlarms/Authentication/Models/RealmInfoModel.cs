using System.Text.Json.Serialization;

namespace LeafletAlarms.Authentication.Models
{
  public class RealmInfoModel
  {
    public string realm { get; set; }
    public string public_key { get; set; }

    [JsonPropertyName("token-service")]
    public string tokenservice { get; set; }

    [JsonPropertyName("account-service")]
    public string accountservice { get; set; }

    [JsonPropertyName("tokens-not-before")]
    public int tokensnotbefore { get; set; }
  }
}
