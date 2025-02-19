using Newtonsoft.Json;

namespace StravaRunner.Models;

public class RefreshTokenResponse
{
    [JsonProperty("token_type")]
    public required string TokenType { get; set; }
    [JsonProperty("access_token")]
    public required string AccessToken { get; set; }
    [JsonProperty("refresh_token")]
    public required string RefreshToken { get; set; }
    [JsonProperty("expires_at")]
    public int ExpiresAt { get; set; }
    [JsonProperty("expires_in")]
    public int ExpiresIn { get; set; }
}