namespace StravaRunner.Core.Models.Settings;

public class EnvSettings
{
    public required string UiUrl { get; init; }
    public required string ApiUrl { get; init; }
    public required string AuthenticateUrl { get; init; }
}