namespace StravaRunner.Core.Models.Data;

public class AuthDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTimeOffset ExpiredAt { get; set; }
}