using System.Text.Json.Serialization;

namespace LeafletAlarms.Authentication.Models
{
    public class Oath2TokenModel
    {
        public string access_token { get; set; }
        public int expires_in { get; set; }
        public int refresh_expires_in { get; set; }
        public string refresh_token { get; set; }
        public string token_type { get; set; }

        [JsonPropertyName("not-before-policy")]
        public int notbeforepolicy { get; set; }
        public string session_state { get; set; }
        public string scope { get; set; }
    }
}
