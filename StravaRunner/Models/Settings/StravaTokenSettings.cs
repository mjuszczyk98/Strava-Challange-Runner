namespace StravaRunner.Models.Settings;

public class StravaSettings
{
    public required string ClientId { get; set; }
    public required string ClientSecret { get; set; }
    public required string AccessToken { get; set; }
    public required string RefreshToken { get; set; }
}