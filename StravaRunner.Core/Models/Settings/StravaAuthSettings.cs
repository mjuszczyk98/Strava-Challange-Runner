namespace StravaRunner.Core.Models.Settings;

public class StravaAuthSettings
{
    public required string ClientId { get; init; }
    public required string ClientSecret { get; init; }
    public required string RefreshToken { get; init; }
}