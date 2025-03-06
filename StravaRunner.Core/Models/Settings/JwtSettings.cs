namespace StravaRunner.Core.Models.Settings;

public class JwtSettings
{
    public required string Key { get; set; }
    public required string Issuer { get; set; }
    public required string Audience { get; set; }
    public int ExpiresInMinutes { get; set; }
    public int RefreshTokenExpiresInDays { get; set; }
}