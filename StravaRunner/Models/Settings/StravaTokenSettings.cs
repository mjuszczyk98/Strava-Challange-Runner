namespace StravaRunner.Models.Settings;

public class StravaTokenSettings
{
    public required string ClientId { get; init; }
    public required string ClientSecret { get; init; }
    public required string AccessToken { get; init; }
    public required string RefreshToken { get; init; }
}